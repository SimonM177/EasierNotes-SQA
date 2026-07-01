import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ControlPanelComponent } from './components/control-panel/control-panel.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ControlPanelComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'Frontend-Angular';
}
