import { Component, OnInit } from '@angular/core';
import { PostService } from '../../services/post.service';
import { PostParameters } from '../../models/post-parameters';
import { PageMeta } from '../../../shared/models/page-meta';
import { ResultWithLinks } from 'src/app/shared/models/result-with-links';
import { Post } from '../../models/post';
 
@Component({
  selector: 'app-post-list',
  templateUrl: './post-list.component.html',
  styleUrls: ['./post-list.component.scss']
})
export class PostListComponent implements OnInit {
  pageMeta: PageMeta;
  posts:Post[];

  postParameter = new PostParameters({ orderBy: 'id desc', pageSize: 10, pageIndex: 0 });

  constructor(private postService:PostService) { }

  ngOnInit() {
    this.getPosts();
  }
  getPosts(){
    this.postService.getPagePosts(this.postParameter).subscribe(resp=>{
      console.log(resp);
      // console.log(resp.headers.get('X-Pagination'));
      this.pageMeta = JSON.parse(resp.headers.get('X-Pagination')) as PageMeta;
      const result = resp.body as ResultWithLinks<Post>;
      this.posts = result.value;
      console.log(this.posts); 
    })
  }

}
