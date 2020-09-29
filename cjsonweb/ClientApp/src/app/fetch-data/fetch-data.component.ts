import { Component } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

@Component({
  selector: 'app-fetch-data',
  templateUrl: './fetch-data.component.html'
})
export class FetchDataComponent {

  apiUrl = 'http://localhost:1212/';
  forecasts = [
    {
      'date': '2020-09-09',
      'temperatureC': 35,
      'summary': 'Freezing'
    },
    {
      'date': '2020-09-10',
      'temperatureC': 53,
      'summary': 'Scorching'
    }
  ];
  header = new HttpHeaders();

  constructor(public http: HttpClient) {

    this.header.set('Access-Control-Allow-Origin', '*');

    this.forecasts = window['CJSON'].compress(this.forecasts); // compress Json
    console.log('Json Data Compressed Client Side ===========>', this.forecasts);

    this.forecasts = window['CJSON'].expand(this.forecasts); // expand Json
    console.log('Json Data Expanded Client Side ===========>', JSON.stringify(this.forecasts));
  }

  convertCJsonToJson() {

    this.http.get(this.apiUrl + 'CJsonToJson', { headers: this.header }).subscribe(result => {

      console.log('Json Data Expanded in API ===========>', result); // json data processed in Api

    }, error => console.error(error));
  }

  convertJsonToCJson() {

    this.http.get(this.apiUrl + 'JsonToCJson', { headers: this.header }).subscribe(result => {

      console.log('Json Data Compressed in API ===========>', result); // cjson data processed in Api

      // Convert CJson to json in client side
      this.forecasts = window['CJSON'].expand(this.forecasts);
      console.log('Json Data Expanded Client Side ===========>', JSON.stringify(this.forecasts));

    }, error => console.error(error));
  }
}

interface WeatherForecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
}
