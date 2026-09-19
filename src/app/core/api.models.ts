// Mirrors the DTOs served by the backend (backend/src/DistrictPortal.Api/Contracts).

export type CollectionStatus = 'success' | 'processing' | 'overdue' | 'not-started' | 'failed';

export interface CollectionSummary {
  id: string;
  name: string;
  status: CollectionStatus;
  statusLabel: string;
  dueDate: string;
  recordCount: number | null;
}

export interface StatTile {
  label: string;
  value: number;
  tone: 'neutral' | 'positive' | 'negative' | 'warning';
}

export type SubmissionStatusValue = 'Uploaded' | 'Processing' | 'Passed' | 'Failed';

export interface SubmissionRecord {
  id: string;
  fileName: string;
  uploadedAt: string;
  uploadedBy: string;
  rowCount: number | null;
  sizeBytes: number;
  status: SubmissionStatusValue;
  passed: boolean;
}

export type OverallStatus = 'Processing' | 'Passed' | 'Failed';

export interface CollectionDetail {
  orgLabel: string;
  id: string;
  name: string;
  schoolYear: string;
  overallStatus: OverallStatus;
  stats: StatTile[];
  lastUploadedFile: SubmissionRecord | null;
  submissionHistory: SubmissionRecord[];
}

export interface LeaDashboard {
  id: string;
  orgLabel: string;
  displayName: string;
  activeCollections: CollectionSummary[];
  inactiveCollections: CollectionSummary[];
}

export type NotificationSeverity = 'info' | 'warning' | 'error';

export interface CollectionNotification {
  id: string;
  severity: NotificationSeverity;
  title: string;
  message: string;
  timestamp: string;
  read: boolean;
  leaId: string | null;
  collectionId: string | null;
}

export type LeaStatusColor = 'red' | 'amber' | 'green' | 'gray';

export interface LeaRosterEntry {
  id: string;
  name: string;
  statusColor: LeaStatusColor;
  overdueCount: number;
  failedCount: number;
  processingCount: number;
  activeCount: number;
  inactiveCount: number;
  matchedCollectionNames: string[];
}

export interface LeaRosterSummary {
  totalLeas: number;
  overdueLeas: number;
  failedLeas: number;
  compliantLeas: number;
}

export interface AdminRosterResponse {
  summary: LeaRosterSummary;
  leas: LeaRosterEntry[];
}

/** Pushed over SignalR (FileProcessingHub) once a submission finishes processing. */
export interface SubmissionStatusChangedMessage {
  leaId: string;
  collectionId: string;
  submission: SubmissionRecord;
  collectionDetail: CollectionDetail;
}
