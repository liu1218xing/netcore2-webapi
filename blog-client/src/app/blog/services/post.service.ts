import { Injectable } from '@angular/core';
import { BaseService } from 'src/app/shared/base.service';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { PostParameters } from '../models/post-parameters';
import { PostAdd } from '../models/post-add';
import { Post } from '../models/post';

@Injectable({
  providedIn: 'root'
})
export class PostService extends BaseService {
  addPost(post: PostAdd) {
    const httpOptions={
      headers:new HttpHeaders({
        'Content-Type': 'application/vnd.cgzl.post.create+json',
        'Accept': 'application/vnd.cgzl.hateoas+json'
      })
    }
    return this.http.post<Post>(`${this.apiUrlBase}/posts`,post,httpOptions)
  }

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
