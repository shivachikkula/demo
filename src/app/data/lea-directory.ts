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
}

export const LEA_ROSTER: LeaRosterEntry[] = [
  {
    id: 'dcps',
    name: 'DCPS',
    statusColor: 'red',
    overdueCount: 1,
    failedCount: 0,
    processingCount: 1,
    activeCount: 4,
    inactiveCount: 7,
  },
  {
    id: 'kipp-dc',
    name: 'KIPP DC',
    statusColor: 'amber',
    overdueCount: 0,
    failedCount: 2,
    processingCount: 0,
    activeCount: 3,
    inactiveCount: 5,
  },
  {
    id: 'friendship-pcs',
    name: 'Friendship PCS',
    statusColor: 'green',
    overdueCount: 0,
    failedCount: 0,
    processingCount: 0,
    activeCount: 2,
    inactiveCount: 6,
  },
  {
    id: 'udc-cc',
    name: 'UDC-CC',
    statusColor: 'gray',
    overdueCount: 0,
    failedCount: 0,
    processingCount: 0,
    activeCount: 1,
    inactiveCount: 3,
  },
];

export const LEA_ROSTER_SUMMARY = {
  totalLeas: 38,
  overdueLeas: 6,
  failedLeas: 4,
  compliantLeas: 21,
};
