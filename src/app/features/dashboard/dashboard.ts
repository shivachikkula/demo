import { UpperCasePipe } from '@angular/common';
import { Component, computed, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

import { CollectionDetail, CollectionSummary } from './dashboard.models';

type CollectionsTab = 'active' | 'inactive';

@Component({
  selector: 'app-dashboard',
  imports: [MatButtonModule, MatIconModule, UpperCasePipe],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard {
  protected readonly districtName = 'District of Columbia Public Schools';

  protected readonly activeTab = signal<CollectionsTab>('active');

  protected readonly activeCollections: CollectionSummary[] = [
    {
      id: 'discipline',
      name: 'Discipline',
      status: 'success',
      statusLabel: 'Success',
      dueDate: 'Sep 15, 2026',
      recordCount: 4908,
    },
    {
      id: 'course',
      name: 'Course',
      status: 'processing',
      statusLabel: 'Processing',
      dueDate: 'Aug 19, 2026',
      recordCount: 2140,
      processingNote: 'Aug 19, 2026',
    },
    {
      id: 'clsd-lea',
      name: 'CLSD-LEA',
      status: 'overdue',
      statusLabel: 'Overdue',
      dueDate: 'Aug 25, 2025',
    },
    {
      id: 'course-phase-ii',
      name: 'Course phase II',
      status: 'not-started',
      statusLabel: 'Not started',
      dueDate: 'Dec 9, 2022',
    },
  ];

  protected readonly inactiveCollections: CollectionSummary[] = [
    { id: 'staff', name: 'Staff', status: 'success', statusLabel: 'Closed', dueDate: 'Jun 30, 2025' },
    {
      id: 'assessment',
      name: 'Assessment',
      status: 'success',
      statusLabel: 'Closed',
      dueDate: 'May 15, 2025',
    },
    {
      id: 'graduation',
      name: 'Graduation',
      status: 'success',
      statusLabel: 'Closed',
      dueDate: 'Apr 1, 2025',
    },
    { id: 'attendance', name: 'Attendance', status: 'success', statusLabel: 'Closed', dueDate: 'Mar 10, 2025' },
    { id: 'enrollment-fall', name: 'Enrollment Fall', status: 'success', statusLabel: 'Closed', dueDate: 'Oct 5, 2024' },
    { id: 'special-ed', name: 'Special Education', status: 'success', statusLabel: 'Closed', dueDate: 'Sep 20, 2024' },
    { id: 'title-i', name: 'Title I', status: 'success', statusLabel: 'Closed', dueDate: 'Aug 1, 2024' },
  ];

  protected readonly collectionDetails: Record<string, CollectionDetail> = {
    discipline: {
      orgLabel: 'DCPS',
      name: 'Discipline Collection',
      schoolYear: 'SY2526',
      overallStatus: 'Passed',
      stats: [
        { label: 'Total records', value: 4908, tone: 'neutral' },
        { label: 'Passed', value: 4822, tone: 'positive' },
        { label: 'Failed', value: 54, tone: 'negative' },
        { label: 'Warnings', value: 32, tone: 'warning' },
      ],
      lastUploadedFile: {
        id: 'f1',
        fileName: 'DCPS_Enrollment_Fall2026_v3.xlsx',
        uploadedAt: 'Aug 10, 2026, 2:32 PM',
        rowCount: 4908,
        sizeKb: 2150,
        passed: true,
      },
      submissionHistory: [
        { id: 's1', fileName: 'Fall2026_v3.xlsx', uploadedAt: 'Aug 10', rowCount: 4908, passed: true },
        { id: 's2', fileName: 'Fall2026_v2.xlsx', uploadedAt: 'Aug 6', rowCount: 4751, passed: false },
        { id: 's3', fileName: 'Fall2026_v1.xlsx', uploadedAt: 'Aug 6', rowCount: 4751, passed: false },
      ],
    },
    course: {
      orgLabel: 'DCPS',
      name: 'Course Collection',
      schoolYear: 'SY2526',
      overallStatus: 'Passed',
      stats: [
        { label: 'Total records', value: 2140, tone: 'neutral' },
        { label: 'Passed', value: 0, tone: 'positive' },
        { label: 'Failed', value: 0, tone: 'negative' },
        { label: 'Warnings', value: 0, tone: 'warning' },
      ],
      lastUploadedFile: {
        id: 'f1',
        fileName: 'DCPS_Course_Fall2026_v1.xlsx',
        uploadedAt: 'Aug 12, 2026, 9:14 AM',
        rowCount: 2140,
        sizeKb: 980,
        passed: true,
      },
      submissionHistory: [
        { id: 's1', fileName: 'Course_Fall2026_v1.xlsx', uploadedAt: 'Aug 12', rowCount: 2140, passed: true },
      ],
    },
    'clsd-lea': {
      orgLabel: 'DCPS',
      name: 'CLSD-LEA Collection',
      schoolYear: 'SY2526',
      overallStatus: 'Failed',
      stats: [
        { label: 'Total records', value: 0, tone: 'neutral' },
        { label: 'Passed', value: 0, tone: 'positive' },
        { label: 'Failed', value: 0, tone: 'negative' },
        { label: 'Warnings', value: 0, tone: 'warning' },
      ],
      lastUploadedFile: {
        id: 'f1',
        fileName: 'No file uploaded',
        uploadedAt: '—',
        rowCount: 0,
        sizeKb: 0,
        passed: false,
      },
      submissionHistory: [],
    },
    'course-phase-ii': {
      orgLabel: 'DCPS',
      name: 'Course Phase II Collection',
      schoolYear: 'SY2526',
      overallStatus: 'Failed',
      stats: [
        { label: 'Total records', value: 0, tone: 'neutral' },
        { label: 'Passed', value: 0, tone: 'positive' },
        { label: 'Failed', value: 0, tone: 'negative' },
        { label: 'Warnings', value: 0, tone: 'warning' },
      ],
      lastUploadedFile: {
        id: 'f1',
        fileName: 'No file uploaded',
        uploadedAt: '—',
        rowCount: 0,
        sizeKb: 0,
        passed: false,
      },
      submissionHistory: [],
    },
  };

  protected readonly selectedCollectionId = signal<string>('discipline');

  protected readonly selectedCollection = computed<CollectionSummary | undefined>(() =>
    this.activeCollections.find((collection) => collection.id === this.selectedCollectionId()),
  );

  protected readonly selectedDetail = computed<CollectionDetail | undefined>(
    () => this.collectionDetails[this.selectedCollectionId()],
  );

  protected readonly isDragging = signal(false);

  protected setTab(tab: CollectionsTab): void {
    this.activeTab.set(tab);
  }

  protected selectCollection(id: string): void {
    this.selectedCollectionId.set(id);
  }

  protected onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragging.set(true);
  }

  protected onDragLeave(): void {
    this.isDragging.set(false);
  }

  protected onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragging.set(false);
  }

  protected formatNumber(value: number): string {
    return value.toLocaleString('en-US');
  }
}
