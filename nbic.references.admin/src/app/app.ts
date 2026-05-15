import { Component } from '@angular/core';
import { SearchComponent } from './search/search';

@Component({
  selector: 'app-root',
  imports: [SearchComponent],
  templateUrl: './app.html',
  standalone: true
})
export class App {}
