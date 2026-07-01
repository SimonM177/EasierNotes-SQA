import { Note } from "./Note";

export interface Category {
  id?: number;
  name: string;
  notes: Note[];
}