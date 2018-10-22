import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { BlogRoutingModule } from './blog-routing.module';
import { MaterialModule } from '../shared/material/material.module';
import { BlogAppComponent } from './blog-app.component';
import { SidenavComponent } from './components/sidenav/sidenav.component';
import { ToolbarComponent } from './components/toolbar/toolbar.component';
import { PostService } from './services/post.service';
import { PostListComponent } from './components/post-list/post-list.component';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { AuthorizationHeaderInterceptor } from '../shared/oidc/authorization-header-interceptor.interceptor';

@NgModule({
  imports: [
    CommonModule,
    BlogRoutingModule,
    MaterialModule
  ],
  declarations: [
    BlogAppComponent, 
    SidenavComponent, 
    ToolbarComponent, PostListComponent],
    providers:[
      PostService,
      {
        provide: HTTP_INTERCEPTORS,
        useClass: AuthorizationHeaderInterceptor,
        multi: true
      }
    ]
})
export class BlogModule { }
