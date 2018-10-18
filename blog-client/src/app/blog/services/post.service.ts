import { Injectable } from '@angular/core';
import { BaseService } from 'src/app/shared/base.service';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { PostParameters } from '../models/post-parameters';

@Injectable({
  providedIn: 'root'
})
export class PostService extends BaseService {

  constructor(private http:HttpClient) {
    super();
   }
   getPagePosts(postParameter?:any | PostParameters){
     return this.http.get(`${this.apiUrlBase}/posts`,{
       headers:new HttpHeaders({
        'Accept': 'application/vnd.cgzl.hateoas+json'
       }),
       observe: 'response',
      params: postParameter
     })
   }
}
