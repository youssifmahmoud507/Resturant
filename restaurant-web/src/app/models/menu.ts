export interface Category { id:number; name:string; subCategories?:{id:number;name:string}[] }
export interface Product { id:number; name:string; description:string; price:number; imageUrl?:string|null; categoryId:number; subCategoryId?:number|null; rating?:number; isFeatured?:boolean; isAvailable?:boolean }
export interface CartLine { product:Product; quantity:number }
