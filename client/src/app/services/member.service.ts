import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Member } from '../models/member.model';
import { Observable, finalize, map, of } from 'rxjs';
import { MemberUpdate } from '../models/member-update.model';
import { UpdateResult } from '../models/helpers/update-result.model';

@Injectable({
  providedIn: 'root'
})
export class MemberService {
  baseUrl: string = environment.apiUrl + 'user';
  members: Member[] = [];
  member: Member | undefined;

  constructor(private http: HttpClient) { }

  getMembers(): Observable<Member[]> {
    if (this.members.length > 0) return of(this.members); // return ObservableOf(members)

    return this.http.get<Member[]>(this.baseUrl).pipe(
      map(members => {
        this.members = members;
        return this.members;
      })
    );
  }

  getMember(email: string): Observable<Member> {
    if (this.members.length > 0) {
      this.member = this.members.find(member => member.email === email);
      if (this.member)
        return of(this.member);
    }

    return this.http.get<Member>(this.baseUrl + '/email/' + email);
  }

  updateMember(memberUpdate: MemberUpdate): Observable<UpdateResult> {
    return this.http.put<UpdateResult>(this.baseUrl, memberUpdate).pipe(
      finalize(() => {
        const member = this.members.find(member => member.email === memberUpdate.email);
        if (member) {
          const index = this.members.indexOf(member);
          this.members[index] = { ...this.members[index], ...memberUpdate } // copy memberUpdate to the list's member
        }
      })
    );
  }

  setMainPhoto(idIn: string | undefined, url_128In: string): Observable<UpdateResult> {
    // cut domain address and convert the address to storage/photos/...
    const dbFormatUrl_128 = url_128In.split('5001/')[1];
    let queryParams = new HttpParams().set('photoUrlIn', dbFormatUrl_128);

    return this.http.put<UpdateResult>(this.baseUrl + '/set-main-photo', null, { params: queryParams })
      .pipe(finalize(() => {
        const member = this.members.find(member => member.id === idIn);
        if (member) {
          member.photos.forEach(photo => {
            if (photo.isMain === true) photo.isMain = false;
            if (photo.url_128 === dbFormatUrl_128) photo.isMain = true;
          })

          const index = this.members.indexOf(member);
          this.members[index] = { ...this.members[index], ...member }
          
          this.member = member;
        }
      }));
  }
}
