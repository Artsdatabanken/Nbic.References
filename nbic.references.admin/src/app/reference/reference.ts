export interface Reference {
  id: string;
  referencePresentation: string;
  referenceType: string;
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
  applicationId: number | null;
  userId: string;
  editDate: string;
}
