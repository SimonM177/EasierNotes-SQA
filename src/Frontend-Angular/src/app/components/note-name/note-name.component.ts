import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  Host,
  HostListener,
  inject,
  input,
  output,
  signal,
} from '@angular/core';

@Component({
  selector: 'app-note-name',
  imports: [],
  templateUrl: './note-name.component.html',
  styleUrl: './note-name.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NoteNameComponent {
  editingMode = signal<boolean>(false);
  inputNoteName = signal<HTMLInputElement | null>(null);
  currentName = input.required<string>();
  viewMode = input.required<string>();
  newName = output<string>();
  elRef = inject(ElementRef);

  enableEdit() {
    this.editingMode.set(true);
    setTimeout(() => {
      const input = document.getElementById(
        'inputNoteName',
      ) as HTMLInputElement;
      this.inputNoteName.set(input);
      this.inputNoteName()?.focus();
    }, 0);
  }

  handleKeyDown(event: KeyboardEvent) {
    const val = this.inputNoteName()?.value.trim();
    console.log('length', val?.length);
    if (!val) return;

    if (event.key === 'Enter') {
      event.preventDefault();
      this.newName.emit(val);
      this.editingMode.set(false);
    }
  }

  @HostListener('document:click', ['$event'])
  onClickOutside(event: MouseEvent) {
    const isClickInside = this.elRef.nativeElement.contains(event.target);

    if (!isClickInside) {
      const val = this.inputNoteName()?.value.trim();
      if (val) {
        this.newName.emit(val);
      }
      this.editingMode.set(false);
    }
  }
}
