import { Component } from '@angular/core';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [],
  templateUrl: './home.html',
  styleUrl: './home.css',
})

export class Home {
  textButton: string = 'Click Me';
  default: string = "";
  
  onButtonClick(text:string) {
    if (!text) {
      alert("Por favor, insira algum texto.");
    }

    if (text){
      alert("Boa! Você digitou: " + text);
    }
  }
}
