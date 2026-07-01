import { inject, Injectable, signal } from '@angular/core';
import { Note } from '../models/Note';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Router } from '@angular/router';
import { CategoriesService } from './categories.service';

@Injectable({
  providedIn: 'root'
})
export class NotesService {

  categoriesService = inject(CategoriesService);
  private http = inject(HttpClient);

  private URLbase = "http://localhost:5219/api/notes";
  private router = inject(Router);

  notes = signal<Note[]>([]);

  constructor() {
      this.getAllNotes();
  }

  public getAllNotes(): void{
    this.http.get<Note[]>(`${this.URLbase}`).subscribe(notes => {
      this.notes.set(notes);
    });
  }
  public create() {
    this.http.post<Note>(`${this.URLbase}/create`, {}).subscribe((response) => {
      this.notes.set([...this.notes(), response]);
      this.router.navigate([`/note/${response.id}`]);
    });
  }

  public update(note:Note){
    return this.http.put(`${this.URLbase}/update`, note).subscribe(() => {
      this.notes.set(this.notes().map(n => n.id === note.id ? note : n));
  });
}

  public delete(note: Note){
    return this.http.delete(`${this.URLbase}/delete/${note.id}`).subscribe(() => {
      this.notes.set(this.notes().filter(n => n.id !== note.id));
  });}

  public getNoteById(id: number): Observable<Note> {
    return this.http.get<Note>(`${this.URLbase}/${id}`);
  }

  public addToCategory(note: Note, categoryId: number) {
    return this.http.patch(`${this.URLbase}/addToCategory/${note.id}/${categoryId}`, {}).subscribe(() => {
      note.categoryId = categoryId;
      this.notes.set(this.notes().map(n => n.id === note.id ? note : n));
    });
  }
  
}

