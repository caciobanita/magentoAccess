using System;
using System.Collections.Generic;
using System.Linq;
using MagentoAccess.Misc;
using MagentoAccess.Models.GetProducts;

namespace MagentoAccess.Models.Services.Soap.GetProducts
{
	[ Serializable ]
	internal class ProductDetails
	{
		public ProductDetails( ProductDetails rp, IEnumerable< MagentoUrl > images = null, string upc = null, string manufacturer = null, decimal? cost = null, string weight = null, string shortDescription = null, string description = null, string price = null, string specialPrice = null, IEnumerable< Category > categories = null, string updatedAt = null )
		{
			this.EntityId = rp.EntityId;
			this.Sku = rp.Sku;
			this.Name = rp.Name;
			this.Qty = rp.Qty;
			var updatedAtParsed = updatedAt.ToDateTimeOrDefault();
			this.UpdatedAt = updatedAtParsed == default(DateTime) ? rp.UpdatedAt : updatedAt;
			this.Categories = categories == null ? rp.Categories : categories.ToArray();

			this.Images = ( images ?? rp.Images ).ToArray();
			decimal temp;
			this.Price = price.ToDecimalOrDefault( out temp ) ? temp : rp.Price;
			this.SpecialPrice = specialPrice.ToDecimalOrDefault( out temp ) ? temp : rp.SpecialPrice;
			this.Description = description ?? rp.Description;
			this.ProductId = rp.ProductId;
			this.Weight = weight ?? rp.Weight;
			this.ShortDescription = shortDescription ?? rp.ShortDescription;

			this.Manufacturer = manufacturer ?? rp.Manufacturer;
			this.Cost = cost ?? rp.Cost;
			this.Upc = upc ?? rp.Upc;
			this.ProductType = rp.ProductType;
		}

		public ProductDetails( Product product )
		{
			var productDeepClone = product.DeepClone();

			this.EntityId = productDeepClone.EntityId;
			this.Sku = productDeepClone.Sku;
			this.Name = productDeepClone.Name;
			this.Qty = productDeepClone.Qty;
			this.UpdatedAt = productDeepClone.UpdatedAt;
			this.Categories = ( productDeepClone.Categories ?? new Models.GetProducts.Category[] { } ).Select( x => new Category( x ) ).ToArray();
			this.Images = ( productDeepClone.Images ?? new List< Models.GetProducts.MagentoUrl >() ).Select( x => new MagentoUrl( x ) ).ToArray();
			this.Price = productDeepClone.Price;
			this.SpecialPrice = productDeepClone.SpecialPrice;
			this.Description = productDeepClone.Description;
			this.ProductId = productDeepClone.ProductId;
			this.Weight = productDeepClone.Weight;
			this.ShortDescription = productDeepClone.ShortDescription;

			this.Manufacturer = productDeepClone.Manufacturer;
			this.Cost = productDeepClone.Cost;
			this.Upc = productDeepClone.Upc;
			this.ProductType = productDeepClone.ProductType;
		}

		public string Upc { get; set; }
		public decimal SpecialPrice { get; set; }
		public decimal Cost { get; set; }
		public string Manufacturer { get; set; }
		public MagentoUrl[] Images { get; set; } //imagento2
		public string ShortDescription { get; set; }
		public string Weight { get; set; } //imagento2
		public string EntityId { get; set; } //id
		public string Sku { get; set; } //imagento2
		public decimal Price { get; set; } //imagento2
		public string Name { get; set; } //imagento2
		public string Description { get; set; }
		public string Qty { get; set; }
		public string ProductId { get; set; } //id
		public Category[] Categories { get; set; } //category_ids have many

		public string ProductType { get; set; }

		public Product ToProduct()
		{
			var productDeepClone = this.DeepClone();
			var product = new Product( productDeepClone.ProductId, productDeepClone.EntityId, productDeepClone.Name, productDeepClone.Sku, productDeepClone.Qty, productDeepClone.Price, productDeepClone.Description, productDeepClone.ProductType, productDeepClone.UpdatedAt );

			product.EntityId = productDeepClone.EntityId;
			product.Categories = productDeepClone.Categories.Select( x => x.ToCategory() ).ToArray();
			product.Images = productDeepClone.Images.Select( x => x.ToMagentoUrl() ).ToArray();
			product.SpecialPrice = productDeepClone.SpecialPrice;
			product.Weight = productDeepClone.Weight;
			product.ShortDescription = productDeepClone.ShortDescription;
			product.Manufacturer = productDeepClone.Manufacturer;
			product.Cost = productDeepClone.Cost;
			product.Upc = productDeepClone.Upc;
			product.ProductType = productDeepClone.ProductType;

			return product;
		}

		public string UpdatedAt { get; set; }
	}
}