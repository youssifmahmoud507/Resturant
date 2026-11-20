import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { Category, Product } from '../models/menu';
@Injectable({providedIn:'root'}) export class MenuService {
 private http=inject(HttpClient); private api='http://localhost:5000/api';
 readonly demoCategories:Category[]=[{id:1,name:'بيتزا',subCategories:[{id:1,name:'لحوم'},{id:2,name:'جبن'}]},{id:2,name:'فطير',subCategories:[{id:3,name:'لحوم'},{id:4,name:'جبن'}]},{id:3,name:'كريب',subCategories:[{id:5,name:'لحوم'},{id:6,name:'جبن'}]},{id:4,name:'باستا'}];
 readonly demoProducts:Product[]=[
 {id:1,name:'بيتزا ميكس لحوم',description:'مزيج من اللحم المفروم والسجق والبيبروني مع جبنة موتزاريلا.',price:185,categoryId:1,subCategoryId:1,rating:4.8,isFeatured:true,imageUrl:'https://images.unsplash.com/photo-1513104890138-7c749659a591'},
 {id:2,name:'بيتزا مارغريتا',description:'صلصة طماطم وموتزاريلا وريحان طازج.',price:135,categoryId:1,subCategoryId:2,rating:4.7,isFeatured:true,imageUrl:'https://images.unsplash.com/photo-1579751626657-72bc17010498'},
 {id:3,name:'فطير مشلتت بالجبنة',description:'فطير مورق محشو بمزيج الجبن.',price:120,categoryId:2,subCategoryId:4,rating:4.6,imageUrl:'https://images.unsplash.com/photo-1604382355076-af4b0eb60143'},
 {id:4,name:'فطير سجق',description:'فطير شرقي بالسجق والفلفل والجبنة.',price:155,categoryId:2,subCategoryId:3,rating:4.8,isFeatured:true,imageUrl:'https://images.unsplash.com/photo-1565299624946-b28f40a0ae38'},
 {id:5,name:'كريب فراخ',description:'كريب مقرمش بحشوة الفراخ والجبنة والصوص.',price:110,categoryId:3,subCategoryId:5,rating:4.7,imageUrl:'https://images.unsplash.com/photo-1565299507177-b0ac66763828'},
 {id:6,name:'كريب ميكس جبن',description:'مزيج شهي من أربع أنواع جبن.',price:105,categoryId:3,subCategoryId:6,rating:4.5,imageUrl:'https://images.unsplash.com/photo-1571407970349-bc81e7e96d47'},
 {id:7,name:'باستا وايت صوص',description:'مكرونة بصوص كريمي ومشروم وقطع فراخ.',price:125,categoryId:4,rating:4.8,isFeatured:true,imageUrl:'https://images.unsplash.com/photo-1473093295043-cdd812d0e601'},
 {id:8,name:'باستا أرابياتا',description:'مكرونة بصوص طماطم حار ولمسة أعشاب.',price:105,categoryId:4,rating:4.6,imageUrl:'https://images.unsplash.com/photo-1551183053-bf91a1d81141'}];
 categories():Observable<Category[]>{return this.http.get<Category[]>(`${this.api}/categories`)}
 products(search=''):Observable<Product[]>{const p=search?new HttpParams().set('search',search):undefined;return this.http.get<Product[]>(`${this.api}/products`,{params:p})}
}
