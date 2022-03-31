import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  baseUrl= environment.apiUrl;

  constructor(private Http: HttpClient) { }

  getUsersWithRoles(){
    return this.Http.get<Partial<User[]>>(this.baseUrl + 'Admin/users-with-roles');
  }

  updateUserRoles(username: string, roles: string[]){
    console.log(username);
    return this.Http.post(this.baseUrl + 'admin/edit-roles/' + username + '?roles=' + roles,{});

  }
}
