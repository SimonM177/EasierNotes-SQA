import { ChangeDetectionStrategy, Component, computed, inject, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NotesService } from '../../services/notes.service';
import { Note } from '../../models/Note';
import { NoteNameComponent } from '../note-name/note-name.component';
import { NoteEditorComponent } from '../note-editor/note-editor.component';
import { CategoriesService } from '../../services/categories.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-note-detail',
  standalone: true,
  imports: [NoteNameComponent, NoteEditorComponent, FormsModule],
  templateUrl: './note-detail.component.html',
  styleUrls: ['./note-detail.component.css'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NoteDetailComponent {
  categoriesService = inject(CategoriesService);
  private noteService = inject(NotesService);
  private route = inject(ActivatedRoute);

  note!: Note;
  categories = computed(() => this.categoriesService.Categories());

  constructor() {
    const idParam = this.route.snapshot.paramMap.get('id');
    const noteId = idParam ? +idParam : NaN;
    this.noteService.getNoteById(noteId).subscribe(note => this.note = note);
    this.categoriesService.getAllCategories();
  }

  changeNoteName(newName: string) {
    if (newName.trim()) {
      this.note.name = newName.trim();
      this.noteService.update(this.note);
    }
  }

  changeCategory(eventTarget: any) {
    const categoryId = parseInt(eventTarget.value);
    if(!categoryId) return;
    this.noteService.addToCategory(this.note, categoryId);
  }

  onSaveHtml(newContent: string) {
    this.note.html = newContent;
    this.noteService.update(this.note);
  }
}
