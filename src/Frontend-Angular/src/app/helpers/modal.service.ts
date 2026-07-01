import { Injectable } from '@angular/core';
import { Modal } from 'bootstrap';

@Injectable({ providedIn: 'root' })
export class ModalService {
  open(modalId: string): void {
    const el = document.getElementById(modalId);
    if (!el) {
      console.warn(`ModalService: no se encontró modal con id="${modalId}"`);
      return;
    }
    Modal.getOrCreateInstance(el).show();
  }

  close(modalId: string): void {
    const el = document.getElementById(modalId);
    if (!el) {
      console.warn(`ModalService: no se encontró modal con id="${modalId}"`);
      return;
    }
    const modal = Modal.getOrCreateInstance(el);
    modal.hide();

    // Limpia el backdrop que quede en el DOM
    document
      .querySelectorAll('.modal-backdrop')
      .forEach((b) => b.remove());

    document.body.classList.remove('modal-open');

    document.body.style.removeProperty('padding-right');
  }
}
