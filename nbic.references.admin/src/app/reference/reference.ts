export interface Reference {
  id: string;
  applicationId: number | null;
  userId: string;
  author: string | null;
  year: string | null;
  title: string | null;
  summary: string | null;
  journal: string | null;
  volume: string | null;
  pages: string | null;
  bibliography: string | null;
  lastname: string | null;
  middlename: string | null;
  firstname: string | null;
  url: string | null;
  keywords: string | null;
  referenceString: string | null;
  referencePresentation: string;
  referenceType: string;
  editDate: string;
}
