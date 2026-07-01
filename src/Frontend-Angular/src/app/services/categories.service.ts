import { inject, Injectable, signal } from '@angular/core';
import { Category } from '../models/Category';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class CategoriesService {
  
  private http = inject(HttpClient);
  private URLbase = "http://localhost:5219/api/categories";
  private router = inject(Router);

  CategoryOpened = signal<Category | null>(null);

  Categories = signal<Category[]>([]);

  public getAllCategories(): void {
    this.http.get<Category[]>(this.URLbase).subscribe((response) => 
      { 
        this.Categories.set(response);
        console.log("Categorias obtenidas:", this.Categories());
       });
  }

  public create(name: string) {
    this.http.post<Category>(`${this.URLbase}/create/${name}`, {}).subscribe((response) => {
      const currentCategories = this.Categories();
      this.Categories.set([...currentCategories, response]);

    });
  }

  public delete(id: number){
    return this.http.delete(`${this.URLbase}/delete/${id}`).subscribe(() => {
      this.Categories.set(this.Categories().filter(c => c.id !== id));
  });}

  
}

