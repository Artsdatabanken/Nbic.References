﻿using System.Text.RegularExpressions;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Nbic.References.Core.Models;

namespace Nbic.References.Infrastructure.Services.Indexing;

public class Index : IDisposable
{
    private const string FieldId = "Id";
    private const string FieldString = "Reference";
    private IndexWriter _writer;

    private bool firstUse = true;
    private HashSet<string> _stopwords =
    [
        "a", "an", "and", "are", "as", "at", "be", "but", "by", "for", "if", "in", "into", "is", "it", "no", "not",
        "of", "on", "or", "such", "that", "the", "their", "then", "there", "these", "they", "this", "to", "was", "will",
        "with"
    ]; // StopAnalyzer.ENGLISH_STOP_WORDS_SET.ToArray().ToHashSet();
    private FSDirectory _dir;

    private static object _theLock = new();
    private bool _lockWasTaken = false;

    public Index(bool waitForLockFile = false, bool deleteAndCreateIndex = false)
    {
        // Ensures index backwards compatibility
        var appLuceneVersion = LuceneVersion.LUCENE_48;

        var applicationRoot = GetApplicationRoot();
        if (string.IsNullOrWhiteSpace(applicationRoot))
        {
            applicationRoot = AppDomain.CurrentDomain.BaseDirectory;
        }
        var indexLocation = applicationRoot.Contains('\\') ? $@"{applicationRoot}\Data\index" : $@"{applicationRoot}/Data/index";
        if (waitForLockFile)
        {
            var lockfileindexLocation = applicationRoot.Contains('\\') ? $@"{applicationRoot}\Data\index\write.lock"
                : $@"{applicationRoot}/Data/index/write.lock";
            //var otherdir = AppDomain.CurrentDomain.BaseDirectory;
            var retry = 50;
            while (retry > 0 && File.Exists(lockfileindexLocation))
            {
                Task.Delay(100).Wait();
                //Thread.Sleep(100);
                retry--;
            }

            Monitor.Enter(_theLock);
            _lockWasTaken = true;
        }



        _dir = FSDirectory.Open(indexLocation);

        //create an analyzer to process the text
        var analyzer = new StandardAnalyzer(appLuceneVersion);
        //_idAnalyser = new SimpleAnalyzer(AppLuceneVersion);

        //create an index writer
        var indexConfig = new IndexWriterConfig(appLuceneVersion, analyzer);
        if (deleteAndCreateIndex)
        {
            indexConfig.OpenMode = OpenMode.CREATE;
        }
        _writer = new IndexWriter(_dir, indexConfig);

    }

    public bool FirstUse
    {
        get => firstUse;
        set => firstUse = value;
    }

    public void AddOrUpdate(Reference reference)
    {
        var indexString = GetIndexString(reference);

        if (indexString.Length <= 0) return;

        var doc = new Document
            {
                new StringField(FieldId, reference.Id.ToString(), Field.Store.YES),
                // StringField indexes but doesn't tokenize
                new TextField(FieldString, indexString, Field.Store.YES)
                //new TextField("favoritePhrase", source.FavoritePhrase, Field.Store.YES)
            };
        //            var id = new Term("Id", reference.Id.ToString());
        //            _writer.DeleteDocuments(id);
        _writer.UpdateDocument(new Term(FieldId, reference.Id.ToString()), doc);
        // _writer.AddDocument(doc);
        //_writer.Flush(triggerMerge: true, applyAllDeletes: true);
        _writer.Commit();
    }

    private static string GetIndexString(Reference reference)
    {
        return string.Join(' ',
            new List<string?> { reference.Firstname, reference.Middlename, reference.Lastname, reference.Summary, reference.Author, reference.Bibliography, reference.Journal, reference.Keywords, reference.Pages, reference.Title, reference.Url, reference.Volume, reference.Year, reference.ReferenceString }
                .Where(x => !string.IsNullOrWhiteSpace(x)).ToArray());
    }

    public void AddOrUpdate(IEnumerable<Reference> refs)
    {
        foreach (var reference in refs)
        {
            var indexString = GetIndexString(reference);

            //if (indexString.Length <= 0) continue;
            var doc = new Document
                                   {
                                       new StringField(FieldId, reference.Id.ToString(), Field.Store.YES),
                                       // StringField indexes but doesn't tokenize
                                       new TextField(FieldString, indexString, Field.Store.YES)
                                       //new TextField("favoritePhrase", source.FavoritePhrase, Field.Store.YES)
                                   };
            //            var id = new Term("Id", reference.Id.ToString());
            //            _writer.DeleteDocuments(id);
            _writer.UpdateDocument(new Term(FieldId, reference.Id.ToString()), doc);

            // _writer.AddDocument(doc);

        }

        //_writer.Flush(triggerMerge: true, applyAllDeletes: true);
        _writer.Commit();
    }

    public void Dispose()
    {
        if (_writer != null)
        {
            _writer.Flush(true, true);
            _writer.WaitForMerges();
            _writer.Commit();

            _writer.Dispose();
        }

        if (_dir != null)
        {
            _dir.Dispose();
        }

        if (_lockWasTaken)
        {
            Monitor.Exit(_theLock);
        }
    }

    public IEnumerable<Guid> SearchReference(string terms, int offset, int limit)
    {
        var lower = terms.ToLower();
        lower = Regex.Replace(lower, @"[,.;:]+", ""); // fjern noen ikke analyserte saker
        lower = Regex.Replace(lower, @"(\s{2,})", "");
        var items = lower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        Query query;

        if (items.Length == 1)
        {
            query = new TermQuery(new Term(FieldString, items[0]));
        }
        else
        {
            query = new BooleanQuery();
            foreach (var item in items)
            {
                if (!_stopwords.Contains(item))
                {
                    ((BooleanQuery)query).Add(new BooleanClause(new TermQuery(new Term(FieldString, item)), Occur.MUST));
                }

            }
        }

        var searcher = new IndexSearcher(_writer.GetReader(applyAllDeletes: true));
        var startAt = (offset * limit);
        var hits = searcher.Search(query, startAt + limit /* top 20 */).ScoreDocs;
        var count = 0;
        var found = new HashSet<Guid>();
        foreach (var hit in hits)
        {
            count++;
            if (count <= startAt) continue;
            var foundDoc = searcher.Doc(hit.Doc);
            var guid = Guid.Parse(foundDoc.Get(FieldId));
            found.Add(guid);
            yield return guid;
        }

        var longTerms = items.Where(x => x.Length > 2).ToArray();

        switch (longTerms.Length)
        {
            case <= 0:
                yield break;
            case 1:
                query = new WildcardQuery(new Term(FieldString, $"{longTerms[0]}*"));
                break;
            default:
            {
                query = new BooleanQuery();
                foreach (var item in longTerms)
                {
                    if (!_stopwords.Contains(item))
                    {
                        ((BooleanQuery)query).Add(new WildcardQuery(new Term(FieldString, $"{item}*")), Occur.MUST);
                    }

                }

                break;
            }
        }

        //query = new WildcardQuery(new Term(Field_String, items[0] + "*"));
        searcher = new IndexSearcher(_writer.GetReader(applyAllDeletes: true));
        hits = searcher.Search(query, startAt + limit /* top 20 */).ScoreDocs;
        foreach (var hit in hits)
        {
            count++;
            if (count <= startAt) continue;
            var foundDoc = searcher.Doc(hit.Doc);
            var guid = Guid.Parse(foundDoc.Get(FieldId));
            if (found.Contains(guid))
            {
                continue;
            }

            yield return guid;
        }
    }

    public void Delete(Guid newGuid)
    {
        _writer.DeleteDocuments(new Term(FieldId, newGuid.ToString()));
        //_writer.Flush(triggerMerge: true, applyAllDeletes: true);
        _writer.Commit();
    }

    public int IndexCount()
    {
        return _writer.MaxDoc;
    }
    public void ClearIndex()
    {
        _writer.DeleteAll();
        //_writer.Flush(true,true);
        _writer.Commit();
    }

    private static string GetApplicationRoot()
    {
        var exePath = Path.GetDirectoryName(System.Reflection
            .Assembly.GetExecutingAssembly().Location);
        var appPathMatcher = new Regex(@"(?<!fil)[A-Za-z]:\\+[\S\s]*?(?=\\+bin)");
        
        if (exePath == null) return string.Empty;
        
        var appRoot = appPathMatcher.Match(exePath).Value;
        return appRoot;
        
    }

}
