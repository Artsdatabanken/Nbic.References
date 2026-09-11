import { Component } from '@angular/core';
import { SearchComponent } from './search/search';

@Component({
  selector: 'app-root',
  imports: [SearchComponent],
  template: `
    <header class="app-header">
      <h1>Nbic References</h1>
    </header>
    <main class="app-main">
      <app-search />
    </main>
  `,
  styles: [
    `.app-header { padding: 16px var(--adb-spacing-md); background: var(--adb-surface-accent, #005A71); color: var(--adb-text-invert, #FFFFFF); }
     .app-header h1 { margin: 0; font-size: var(--adb-font-size-7, 1.5rem); font-weight: var(--adb-font-weight-bold, 700); }
     .app-main { background: var(--adb-surface-primary, #FFFFFF); min-height: calc(100vh - 64px); }`
  ],
  standalone: true
})
export class App {}
