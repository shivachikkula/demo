export type CollectionStatus = 'success' | 'processing' | 'overdue' | 'not-started';

export interface CollectionSummary {
  id: string;
  name: string;
  status: CollectionStatus;
  statusLabel: string;
  dueDate: string;
  recordCount?: number;
  processingNote?: string;
}

export interface SubmissionFile {
  id: string;
  fileName: string;
  uploadedAt: string;
  rowCount: number;
  passed: boolean;
}

export interface StatTile {
  label: string;
  value: number;
  tone: 'neutral' | 'positive' | 'negative' | 'warning';
}

export interface CollectionDetail {
  orgLabel: string;
  name: string;
  schoolYear: string;
  overallStatus: 'Passed' | 'Failed';
  stats: StatTile[];
  lastUploadedFile: SubmissionFile & { sizeKb: number };
  submissionHistory: SubmissionFile[];
}

export type NotificationSeverity = 'info' | 'warning' | 'error';

export interface CollectionNotification {
  id: string;
  severity: NotificationSeverity;
  title: string;
  message: string;
  timestamp: string;
  read: boolean;
  collectionId?: string;
}
