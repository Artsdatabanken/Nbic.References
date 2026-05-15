import { Component, signal, computed } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ReferencesService } from '../references/references.service';
import { Reference } from '../reference/reference';

const PAGE_SIZE = 12;

@Component({
  selector: 'app-search',
  imports: [FormsModule],
  templateUrl: './search.html',
  styleUrl: './search.css',
  standalone: true
})
export class SearchComponent {
  searchTerm = signal('');
  paginatedResults = signal<Reference[]>([]);
  loading = signal(false);
  errorMessage = signal('');
  currentPage = signal(1);
  resultCount = signal(0);
  hasMore = signal(false);
  countKnown = signal(false);
  lowerBound = signal(0);

  readonly totalPages = computed(() => {
    if (this.countKnown()) {
      return Math.max(1, Math.ceil(this.resultCount() / PAGE_SIZE));
    }
    // Lower bound + 1 (we know there's at least one more page)
    return Math.max(this.currentPage() + 1, Math.floor(this.lowerBound() / PAGE_SIZE) + 1);
  });

  readonly visiblePages = computed(() => {
    const total = this.totalPages();
    const current = this.currentPage();
    const delta = 2;
    const start = Math.max(1, current - delta);
    const end = Math.min(total, current + delta);
    const pages: number[] = [];
    for (let i = start; i <= end; i++) pages.push(i);
    return pages;
  });

  readonly showPagination = computed(() => this.countKnown() && this.totalPages() > 1);

  constructor(private service: ReferencesService) {
    this.performSearch();
  }

  onSearchChanged() {
    this.currentPage.set(1);
    this.performSearch();
  }

  async performSearch() {
    this.loading.set(true);
    this.errorMessage.set('');
    const page = this.currentPage();
    const search = this.searchTerm().trim() || null;
    const offset = (page - 1) * PAGE_SIZE;

    try {
      const results = await this.service.getAll(offset, PAGE_SIZE, search).toPromise();
      if (!results) {
        this.errorMessage.set('No results returned.');
        return;
      }
      this.paginatedResults.set([...results]);
      this.hasMore.set(results.length === PAGE_SIZE);
      this.resultCount.set(results.length);

      if (page === 1) {
        if (results.length < PAGE_SIZE) {
          // Full result set known
          this.countKnown.set(true);
          this.lowerBound.set(results.length);
        } else {
          this.countKnown.set(false);
          this.lowerBound.set(PAGE_SIZE);
        }
      } else {
        this.hasMore.set(results.length === PAGE_SIZE);
        if (results.length < PAGE_SIZE) {
          // Last partial page — we now know the total
          this.lowerBound.set((page - 1) * PAGE_SIZE + results.length);
          this.countKnown.set(true);
        } else {
          this.lowerBound.set(page * PAGE_SIZE);
          this.countKnown.set(false);
        }
      }
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : 'Search failed';
      this.errorMessage.set(msg);
    } finally {
      this.loading.set(false);
    }
  }

  goToPage(page: number) {
    this.currentPage.set(page);
    this.performSearch();
  }

  getPage(n: number): number {
    return Math.max(1, Math.min(n, this.totalPages()));
  }
}
