import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-control-panel',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './control-panel.component.html',
  styleUrl: './control-panel.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ControlPanelComponent {
  
}
