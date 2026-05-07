import { Component, effect, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Store } from '@ngrx/store';
import { uiFeature } from './store/ui/ui.reducer';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private readonly store = inject(Store);
  readonly theme = this.store.selectSignal(uiFeature.selectTheme);

  constructor() {
    effect(() => {
      const cls = document.body.classList;
      if (this.theme() === 'dark') cls.add('theme-dark');
      else cls.remove('theme-dark');
    });
  }
}
