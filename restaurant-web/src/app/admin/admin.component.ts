import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';

const API = 'http://localhost:5000/api';
type Tab = 'overview' | 'products' | 'orders' | 'customers';
interface Category { id:number; name:string; subCategories?:{id:number;name:string}[] }
interface Product { id:number; name:string; description:string; price:number; imageUrl?:string; categoryId:number; categoryName?:string; subCategoryId?:number|null; subCategoryName?:string|null; isAvailable:boolean; isFeatured:boolean }
interface Order { id:number; createdAt:string; status:string; total:number; customerName:string; deliveryAddress:string; items:{productName:string;unitPrice:number;quantity:number}[] }
interface Customer { id:number; fullName:string; phone:string; email:string; isActive:boolean; createdAt:string; ordersCount:number }
@Component({selector:'app-admin',standalone:true,imports:[CommonModule,FormsModule],templateUrl:'./admin.component.html',styleUrl:'./admin.component.css'})
export class AdminComponent {
 private http=inject(HttpClient);
 token=sessionStorage.getItem('haramain_admin_token')||'';
 tab=signal<Tab>('overview'); loading=signal(false); error=signal(''); success=signal('');
 email='admin@alharamain2.local'; password='ChangeMe123!';
 summary=signal<any>(null); categories=signal<Category[]>([]); products=signal<Product[]>([]); orders=signal<Order[]>([]); customers=signal<Customer[]>([]);
 productSearch=signal(''); customerSearch=signal('');
 editingId=signal<number|null>(null); form={name:'',description:'',price:0,imageUrl:'',categoryId:1,subCategoryId:null as number|null,isAvailable:true,isFeatured:false};
 filteredProducts=computed(()=>this.products().filter(p=>`${p.name} ${p.categoryName||''} ${p.subCategoryName||''}`.toLowerCase().includes(this.productSearch().toLowerCase())));
 constructor(){if(this.token)this.loadAll()}
 headers(){return new HttpHeaders({Authorization:`Bearer ${this.token}`})}
 login(){this.error.set('');this.http.post<any>(`${API}/auth/login`,{email:this.email,password:this.password}).subscribe({next:r=>{if(r.role!=='Admin'){this.error.set('الحساب ده مش Admin');return}this.token=r.token;sessionStorage.setItem('haramain_admin_token',this.token);this.loadAll()},error:()=>this.error.set('بيانات الدخول غير صحيحة أو الـ API مش شغال')})}
 logout(){this.token='';sessionStorage.removeItem('haramain_admin_token');this.summary.set(null)}
 loadAll(){this.loading.set(true);const h=this.headers();this.http.get<any>(`${API}/admin/dashboard/summary`,{headers:h}).subscribe({next:x=>this.summary.set(x),error:()=>this.handleError()});this.http.get<Category[]>(`${API}/categories`).subscribe({next:x=>this.categories.set(x),error:()=>{}});this.refreshProducts();this.refreshOrders();this.refreshCustomers();this.loading.set(false)}
 refreshProducts(){this.http.get<Product[]>(`${API}/admin/products`,{headers:this.headers()}).subscribe({next:x=>this.products.set(x),error:()=>this.handleError()})}
 refreshOrders(){this.http.get<Order[]>(`${API}/orders`,{headers:this.headers()}).subscribe({next:x=>this.orders.set(x),error:()=>this.handleError()})}
 refreshCustomers(){this.http.get<Customer[]>(`${API}/admin/customers`,{headers:this.headers()}).subscribe({next:x=>this.customers.set(x),error:()=>this.handleError()})}
 handleError(){this.error.set('حصل خطأ في الاتصال. تأكد إن Backend شغال وإنك مسجل دخول كأدمن.');this.loading.set(false)}
 setTab(t:Tab){this.tab.set(t);this.success.set('');this.error.set('')}
 resetForm(){this.editingId.set(null);this.form={name:'',description:'',price:0,imageUrl:'',categoryId:this.categories()[0]?.id||1,subCategoryId:null,isAvailable:true,isFeatured:false}}
 edit(p:Product){this.editingId.set(p.id);this.form={name:p.name,description:p.description,price:p.price,imageUrl:p.imageUrl||'',categoryId:p.categoryId,subCategoryId:p.subCategoryId||null,isAvailable:p.isAvailable,isFeatured:p.isFeatured};this.tab.set('products');window.scrollTo({top:0,behavior:'smooth'})}
 saveProduct(){const id=this.editingId();const req=id?this.http.put(`${API}/admin/products/${id}`,this.form,{headers:this.headers()}):this.http.post(`${API}/admin/products`,this.form,{headers:this.headers()});req.subscribe({next:()=>{this.success.set(id?'تم تعديل المنتج':'تمت إضافة المنتج');this.resetForm();this.refreshProducts();this.loadSummary()},error:()=>this.error.set('تعذر حفظ المنتج. راجع البيانات والاتصال.')})}
 deleteProduct(p:Product){if(!confirm(`تأكيد حذف/إخفاء ${p.name}؟`))return;this.http.delete(`${API}/admin/products/${p.id}`,{headers:this.headers()}).subscribe({next:()=>{this.success.set('تم حذف المنتج أو إخفاؤه من المنيو');this.refreshProducts();this.loadSummary()},error:()=>this.handleError()})}
 updateOrder(o:Order,status:string){this.http.patch(`${API}/orders/${o.id}/status`,{status},{headers:this.headers()}).subscribe({next:()=>{o.status=status;this.success.set(`تم تحديث حالة الطلب #${o.id}`);this.loadSummary()},error:()=>this.handleError()})}
 toggleCustomer(c:Customer){this.http.patch(`${API}/admin/customers/${c.id}/status`,{isActive:!c.isActive},{headers:this.headers()}).subscribe({next:()=>{c.isActive=!c.isActive;this.success.set('تم تحديث حالة حساب العميل')},error:()=>this.handleError()})}
 deleteCustomer(c:Customer){if(!confirm(`تأكيد حذف/إيقاف حساب ${c.fullName}؟`))return;this.http.delete(`${API}/admin/customers/${c.id}`,{headers:this.headers()}).subscribe({next:()=>{this.refreshCustomers();this.success.set('تم تنفيذ الإجراء مع الحفاظ على سجل الطلبات')},error:()=>this.handleError()})}
 loadSummary(){this.http.get<any>(`${API}/admin/dashboard/summary`,{headers:this.headers()}).subscribe({next:x=>this.summary.set(x),error:()=>{}})}
 subs(){return this.categories().find(c=>c.id===Number(this.form.categoryId))?.subCategories||[]}
}
