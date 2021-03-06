﻿using System;
using System.Linq;
using FluentAssertions;
using MagentoAccess.Misc;
using MagentoAccess.Models.PutInventory;
using MagentoAccess.Models.Services.Soap.PutStockItems;
using MagentoAccess.Services;
using MagentoAccess.Services.Soap._1_14_1_0_ee;
using MagentoAccess.Services.Soap._1_7_0_1_ce_1_9_0_1_ce;
using MagentoAccessTestsIntegration.TestEnvironment;
using NUnit.Framework;

namespace MagentoAccessTestsIntegration.Services
{
	[TestFixture]
	[Ignore("Since Test cases used for MagentoService")]
	internal class MeagentoServiceLowLevelSoap1_14_1_EE_Test : BaseTest
	{
		[ Test ]
		public void GetOrders_ByDatesStoreContainsOrders_ReceiveOrders()
		{
			//------------ Arrange

			//------------ Act
			var firstCreatedItem = this._orders.OrderBy( x => x.UpdatedAt.ToDateTimeOrDefault() ).First();
			var lastCreatedItem = this._orders.OrderBy( x => x.UpdatedAt.ToDateTimeOrDefault() ).Last();

			var modifiedFrom = DateTime.Parse( firstCreatedItem.UpdatedAt ).AddSeconds( 1 );
			var modifiedTo = DateTime.Parse( lastCreatedItem.UpdatedAt ).AddSeconds( -1 );

			var getOrdersTask = this._magentoServiceLowLevelSoapV11410Ee.GetOrdersAsync( modifiedFrom, modifiedTo );
			getOrdersTask.Wait();

			//------------ Assert
			var thatMustBeReturned = this._orders.Where( x => x != firstCreatedItem && x != lastCreatedItem ).Select( x => x.incrementId ).ToList();
			var thatWasReturned = getOrdersTask.Result.Orders.ToList().Select( x => x.incrementId ).ToList();

			thatWasReturned.Should().BeEquivalentTo( thatMustBeReturned );

			getOrdersTask.Result.Orders.ShouldBeEquivalentTo( this._orders.Take( this._orders.Count() - 1 ).Skip( 1 ) );
		}

		[ Test ]
		public void GetOrders_ByIdsStoreContainsOrders_ReceiveOrders()
		{
			//------------ Arrange

			//------------ Act
			//var ordersIds = new List< string >() { "100000001", "100000002" };
			var ordersIds = this._orders.Select( x => x.incrementId ).ToList();

			var getOrdersTask = this._magentoServiceLowLevelSoapV11410Ee.GetOrdersAsync( ordersIds );
			getOrdersTask.Wait();

			//------------ Assert
			getOrdersTask.Result.Orders.ShouldBeEquivalentTo( this._orders );
		}

		[ Test ]
		public void GetProducts_StoreContainsProducts_ReceiveProducts()
		{
			//------------ Arrange

			//------------ Act
			var getProductsTask = this._magentoServiceLowLevelSoapV11410Ee.GetProductsAsync();
			getProductsTask.Wait();

			//------------ Assert
			getProductsTask.Result.Products.Should().NotBeEmpty();
		}

		[ Test ]
		public void GetStockItems_StoreContainsStockItems_ReceiveStockItems()
		{
			//------------ Arrange

			//------------ Act
			//var skusorids = new List< string >() { "501shirt", "311" };
			var skusorids = this._productsIds[_magentoServiceLowLevelSoapV11410Ee.BaseMagentoUrl].Select((kv, i) => i % 2 == 0 ? kv.Key.ToString() : kv.Value).ToList();

			var getProductsTask = this._magentoServiceLowLevelSoapV11410Ee.GetStockItemsAsync( skusorids );
			getProductsTask.Wait();

			//------------ Assert
			getProductsTask.Result.InventoryStockItems.Select( x => x.ProductId ).ShouldBeEquivalentTo( this._productsIds.Select( x => x.Key ) );
		}

		[ Test ]
		public void GetSessionId_StoreContainsUser_ReceiveSessionId()
		{
			//------------ Arrange

			//------------ Act
			var getProductsTask = this._magentoServiceLowLevelSoapV11410Ee.GetSessionId( false );
			getProductsTask.Wait();

			//------------ Assert
			getProductsTask.Result.Should().NotBeNull();
		}

		[ Test ]
		public void GetSessionId_IncorrectApiUser_NoExceptionThrowns()
		{
			//------------ Arrange

			//------------ Act

			Action act = () =>
			{
				var service = new MagentoServiceLowLevelSoap_v_1_14_1_0_EE(
					"incorrect api user",
					this._testData.GetMagentoSoapUser().ApiKey,
					this._testData.GetMagentoUrls().MagentoBaseUrl,
					null );

				var getProductsTask = service.GetSessionId( false );
				getProductsTask.Wait();
			};

			//------------ Assert

			act.ShouldNotThrow();
		}

		[ Test ]
		public void UpdateInventory_StoreWithItems_ItemsUpdated()
		{
			//------------ Arrange

			//------------ Act

			var productsAsync = this._magentoServiceLowLevelSoapV11410Ee.GetStockItemsAsync(this._productsIds[_magentoServiceLowLevelSoapV11410Ee.BaseMagentoUrl].Select(x => x.Value).ToList());
			productsAsync.Wait();

			var qtyUpdateFirsttime = 123;
			var itemsToUpdate = productsAsync.Result.InventoryStockItems.Select( x => new PutStockItem( new Inventory() { Qty = qtyUpdateFirsttime, ProductId = x.ProductId } ) ).ToList();

			var getProductsTask = this._magentoServiceLowLevelSoapV11410Ee.PutStockItemsAsync( itemsToUpdate );
			getProductsTask.Wait();

			////

			var productsAsync2 = this._magentoServiceLowLevelSoapV11410Ee.GetStockItemsAsync(this._productsIds[_magentoServiceLowLevelSoapV11410Ee.BaseMagentoUrl].Select(x => x.Value).ToList());
			productsAsync2.Wait();

			var qtyUpdateSecondTime = 100500;
			var itemsToUpdate2 = productsAsync2.Result.InventoryStockItems.Select( x => new PutStockItem( new Inventory() { Qty = qtyUpdateSecondTime, ProductId = x.ProductId } ) ).ToList();

			var getProductsTask2 = this._magentoServiceLowLevelSoapV11410Ee.PutStockItemsAsync( itemsToUpdate2 );
			getProductsTask2.Wait();

			//------------ Assert
			var productsAsync3 = this._magentoServiceLowLevelSoapV11410Ee.GetStockItemsAsync(this._productsIds[_magentoServiceLowLevelSoapV11410Ee.BaseMagentoUrl].Select(x => x.Value).ToList());
			productsAsync3.Wait();

			productsAsync2.Result.InventoryStockItems.Should().OnlyContain( x => x.Qty.ToDecimalOrDefault() == qtyUpdateFirsttime );
			productsAsync3.Result.InventoryStockItems.Should().OnlyContain( x => x.Qty.ToDecimalOrDefault() == qtyUpdateSecondTime );
		}

		[ Test ]
		public void GetMagentoInfoAsync_StoreExist_StoreVersionRecived()
		{
			//------------ Arrange

			//------------ Act

			var productsAsync = this._magentoServiceLowLevelSoapV11410Ee.GetMagentoInfoAsync();
			productsAsync.Wait();

			//------------ Assert
			productsAsync.Result.MagentoVersion.Should().NotBeNullOrWhiteSpace();
		}
	}
}