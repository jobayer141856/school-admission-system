export interface AdmissionClassItem {
  id: string;
  name: string;
  totalSeats: number;
  heldSeats: number;
  confirmedSeats: number;
  availableSeats: number;
}

export interface CreateClassPayload {
  name: string;
  totalSeats: number;
}

export interface AdmissionClassCreated {
  id: string;
  name: string;
  totalSeats: number;
}

