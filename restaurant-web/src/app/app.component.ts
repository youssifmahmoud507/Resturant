import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MenuService } from './core/menu.service';
import { CartLine, Category, Product } from './models/menu';
import { AdminComponent } from './admin/admin.component';
@Component({selector:'app-root',standalone:true,imports:[CommonModule,FormsModule,AdminComponent],templateUrl:'./app.component.html'})
export class AppComponent {
 isAdmin=signal(window.location.pathname.startsWith('/admin'));
 private menu=inject(MenuService); categories=signal<Category[]>(this.menu.demoCategories); products=signal<Product[]>(this.menu.demoProducts);
 selectedCategory=signal(0); selectedSub=signal(0); search=signal(''); cart=signal<CartLine[]>([]); cartOpen=signal(false); accountOpen=signal(false); activeTab=signal('menu'); coupon=signal(''); couponMessage=signal(''); discount=signal(0); note=signal(''); customerName=''; phone=''; address=''; payment='vodafone';
 filtered=computed(()=>this.products().filter(p=>(!this.selectedCategory()||p.categoryId===this.selectedCategory())&&(!this.selectedSub()||p.subCategoryId===this.selectedSub())&&(`${p.name} ${p.description}`).includes(this.search().trim())));
 featured=computed(()=>this.products().filter(p=>p.isFeatured)); count=computed(()=>this.cart().reduce((s,x)=>s+x.quantity,0)); subtotal=computed(()=>this.cart().reduce((s,x)=>s+x.product.price*x.quantity,0)); total=computed(()=>Math.max(0,this.subtotal()-this.discount()));
 constructor(){if(!this.isAdmin()){this.menu.categories().subscribe({next:x=>this.categories.set(x),error:()=>{}});this.menu.products().subscribe({next:x=>{if(x?.length)this.products.set(x)},error:()=>{}})}}
 selectCategory(id:number){this.selectedCategory.set(id);this.selectedSub.set(0);this.activeTab.set('menu')}
 add(p:Product){this.cart.update(items=>{const found=items.find(x=>x.product.id===p.id);return found?items.map(x=>x.product.id===p.id?{...x,quantity:x.quantity+1}:x):[...items,{product:p,quantity:1}]})}
 changeQty(id:number,delta:number){this.cart.update(items=>items.map(x=>x.product.id===id?{...x,quantity:x.quantity+delta}:x).filter(x=>x.quantity>0))}
 applyCoupon(){const c=this.coupon.trim().toUpperCase();if(c==='HARAMAIN10'){this.discount.set(this.subtotal()*.1);this.couponMessage.set('تم تطبيق خصم 10% بنجاح')}else{this.discount.set(0);this.couponMessage.set('الكود غير صحيح أو غير متاح')}}
 whatsapp(){if(!this.customerName||!this.phone||!this.address){alert('من فضلك اكتب الاسم ورقم الهاتف والعنوان');return}const lines=this.cart().map(x=>`${x.product.name} × ${x.quantity} = ${x.product.price*x.quantity} ج`).join('%0A');const msg=`طلب جديد - الحرمين 2%0Aالاسم: ${encodeURIComponent(this.customerName)}%0Aالهاتف: ${encodeURIComponent(this.phone)}%0Aالعنوان: ${encodeURIComponent(this.address)}%0A${lines}%0Aالإجمالي: ${this.total()} ج%0Aطريقة الدفع: ${this.payment}%0Aملاحظات: ${encodeURIComponent(this.note)}`;window.open(`https://wa.me/?text=${msg}`,'_blank')}
}
