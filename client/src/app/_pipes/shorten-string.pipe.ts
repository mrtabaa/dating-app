import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'shortenString'
})
export class ShortenStringPipe implements PipeTransform {

  transform(value: string, limit?: number): string | null {
    if (!value)
      return null;
    
    return limit ? value.substring(0, limit) : value.substring(0, 35); // set 35 for website URL lengths
  }

}
