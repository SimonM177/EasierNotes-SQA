import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  OnInit,
  AfterViewInit,
  signal,
  HostListener,
} from '@angular/core';
import { NotesService } from '../../services/notes.service';
import { Note } from '../../models/Note';
import { Router } from '@angular/router';
import { ModalService } from '../../helpers/modal.service';
import { NoteNameComponent } from '../note-name/note-name.component';
import { displayContentOverview } from '../../helpers/showNoteOverview';
import { CategoriesService } from '../../services/categories.service';
import { Category } from '../../models/Category';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-note-list',
  imports: [NoteNameComponent, FormsModule],
  templateUrl: './note-list.component.html',
  styleUrl: './note-list.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NoteListComponent implements OnInit, AfterViewInit {
  notesService = inject(NotesService);
  categoriesService = inject(CategoriesService);
  modalService = inject(ModalService);
  router = inject(Router);

  searchTerm = signal<string>('');

  // Funcion para mostrar una vista previa de la nota
  displayContentOverview = displayContentOverview;

  ngOnInit() {
    //Se obtienen todas las notas
    this.notesService.getAllNotes();

    //Se obtienen todas las categorias
    if(this.categoriesService.Categories().length === 0){
      this.categoriesService.getAllCategories();
    }

    // Si se ha seleccionado una categoria, se obtienen las notas de la categoria
    const categoryOpened = this.categoriesService.CategoryOpened();
    if(categoryOpened){
      this.categoriesSelected.set([categoryOpened]);
    }

    //Se reestablece la categoria seleccionada
    this.categoriesService.CategoryOpened.set(null);

  }
  ngAfterViewInit() {
    //Se actualizan los select de las notas
    this.notes().forEach(note => {
      const select = document.getElementById(`select-category-${note.id}`) as HTMLSelectElement;
      if(select) select.value = this.getCategoryName(note.categoryId);
    });
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event) {
    const target = event.target as HTMLElement;
    if(!target.closest('.category-selector') && !target.closest('.filter-button')){
      this.categorySelectorOpen.set(false);
    }
  }

  notes = computed(() =>{

    // Se filtran las notas por las categorias seleccionadas
    const notesFilteredByCategory = this.categoriesSelected().length > 0 ? this.notesService.notes().filter(note => 
        this.categoriesSelected().some(category => category.id === note.categoryId)) : this.notesService.notes();

    //Luego, se filtran por el termino de busqueda
    if(this.searchTerm().length === 0) return notesFilteredByCategory;
    else return notesFilteredByCategory.filter(note =>
       note.name.toLowerCase().trim().includes(this.searchTerm().toLowerCase().trim()));
  });

  // Se obtiene el nombre de las categorias para mostrar en el panel de filtro
  categoriesNames = computed(() => {
    return this.categoriesService.Categories().map(category => category.name);
  });


  categoriesSelected = signal<Category[]>([]);

  toggleCategoryCheckbox(name: string) {

    const category = this.categoriesService.Categories().find(category => category.name === name);
    if(!category) return;

    // Si la categoria ya esta seleccionada, se desmarca
    if(this.categoriesSelected().some(categ => categ.id === category.id)){
      this.categoriesSelected.set(this.categoriesSelected().filter(categ => categ.id !== category.id));
    }
    // Si la categoria no esta seleccionada, se marca
    else{
      this.categoriesSelected.set([...this.categoriesSelected(), category]);
    }
  }

  // Se controla si el selector de categorias esta abierto
  categorySelectorOpen = signal<boolean>(false);
  toggleCategorySelectorOpen() {

    //Marcamos todas las categorias que esten en categoriesSelected
    if(!this.categorySelectorOpen()){
    this.categoriesSelected().forEach(category => {
      const input = document.getElementById(`check-category-${category.name}`) as HTMLInputElement;
      if(input) input.checked = true;
    });
  }

    //Se controla si el selector de categorias esta abierto
    this.categorySelectorOpen.set(!this.categorySelectorOpen());
    console.log('toggleCategorySelectorOpen', this.categorySelectorOpen());
  }

  filterButtonText = computed(() => {
    if(this.categoriesSelected().length === 0)
       return 'Todas';
    else
      return this.categoriesSelected().map(category => category.name).join(', ');
  });
  
  selectedNote = signal<Note | null>(null);
  
  getCategoryName(categoryId: number): string {
    const category = this.categoriesService.Categories().find(category => category.id === categoryId);
    return category?.name || '';
  }

  changeCategory(note: Note, eventTarget: any) {
    const value = eventTarget.value;
    const categoryId = parseInt(value);
    if(!categoryId) return;
    this.notesService.addToCategory(note, categoryId);
  }


  updateSearchTerm(value: string | null) {
    if (value === null) this.searchTerm.set('');
    else this.searchTerm.set(value);
  }

  private unselectNote() {
    this.selectedNote.set(null);
  }

  selectNote(selectedNote: Note) {
    if (selectedNote.id === this.selectedNote()?.id) {
      this.unselectNote();
    } else {
      this.selectedNote.set(selectedNote);
    }
  }

  isSelected(currentNote: Note): boolean {
    let result = false;
    if (currentNote.id === this.selectedNote()?.id) {
      result = true;
    }
    return result;
  }

  openNote(noteId: number) {
    this.router.navigate(['/note', noteId]);
  }

  openDeleteModal() {
    console.log('deleteNote');
    if (this.selectedNote() === null) return;
    this.modalService.open('confirmDeleteModal');
  }

  deleteNote() {
    if (this.selectedNote() !== null) {
      this.notesService.delete(this.selectedNote()!);
      this.unselectNote();
    }
  }

  createNote() {
    this.notesService.create();
  }

  changeNoteName(newName: string, note: Note) {
    if (newName && newName.trim() !== '') {
      note.name = newName;
      console.log('cambiando nombre de nota', note);
      this.notesService.update(note);
    }
  }
}
