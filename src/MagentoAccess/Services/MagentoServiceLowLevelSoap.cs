﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using MagentoAccess.MagentoSoapServiceReference;

namespace MagentoAccess.Services
{
	internal class MagentoServiceLowLevelSoap
	{
		protected readonly Mage_Api_Model_Server_Wsi_HandlerPortTypeClient _magentoSoapService;

		protected string UserName { get; set; }

		protected string Password { get; set; }

		protected string Store { get; set; }

		protected string _sessionId;

		protected DateTime _sessionIdCreatedAt;

		protected const int SessionIdLifeTime = 3590;

		protected async Task< string > GetSessionId()
		{
			if( string.IsNullOrWhiteSpace( this._sessionId ) && DateTime.UtcNow.Subtract( this._sessionIdCreatedAt ).TotalSeconds < SessionIdLifeTime )
				return this._sessionId;

			var getSessionIdTask = await this._magentoSoapService.loginAsync( this.UserName, this.Password ).ConfigureAwait( false );
			this._sessionIdCreatedAt = DateTime.UtcNow;
			return this._sessionId = getSessionIdTask.result;
		}

		public MagentoServiceLowLevelSoap( string userName, string password, string endpointAddress, string store )
		{
			this.UserName = userName;
			this.Password = password;
			this.Store = store;
			this._magentoSoapService = new Mage_Api_Model_Server_Wsi_HandlerPortTypeClient( new BasicHttpBinding() { MaxReceivedMessageSize = Int32.MaxValue }, new EndpointAddress( endpointAddress ) );
		}

		public async Task< salesOrderListResponse > GetOrdersAsync( DateTime modifiedFrom, DateTime modifiedTo )
		{
			var sessionId = await this.GetSessionId().ConfigureAwait( false );
			filters filters;

			if( string.IsNullOrWhiteSpace( this.Store ) )
				filters = new filters { complex_filter = new complexFilter[ 2 ] };
			else
			{
				filters = new filters { complex_filter = new complexFilter[ 3 ] };
				filters.complex_filter[ 1 ] = new complexFilter() { key = "store_id", value = new associativeEntity() { key = "in", value = this.Store } };
			}

			filters.complex_filter[ 0 ] = new complexFilter() { key = "updated_at", value = new associativeEntity() { key = "from", value = modifiedFrom.ToString() } };
			filters.complex_filter[ 1 ] = new complexFilter() { key = "updated_at", value = new associativeEntity() { key = "to", value = modifiedTo.ToString() } };

			var res = await this._magentoSoapService.salesOrderListAsync( sessionId, filters ).ConfigureAwait( false );

			return res;
		}

		public async Task< salesOrderListResponse > GetOrdersAsync( IEnumerable< string > ordersIds )
		{
			var sessionId = await this.GetSessionId().ConfigureAwait( false );
			filters filters;
			if( string.IsNullOrWhiteSpace( this.Store ) )
				filters = new filters { complex_filter = new complexFilter[ 1 ] };
			else
			{
				filters = new filters { complex_filter = new complexFilter[ 2 ] };
				filters.complex_filter[ 1 ] = new complexFilter() { key = "store_id", value = new associativeEntity() { key = "in", value = this.Store } };
			}

			filters.complex_filter[ 0 ] = new complexFilter() { key = "increment_id", value = new associativeEntity() { key = "in", value = ordersIds.Aggregate( ( ac, x ) => ac += "," + x ) } };

			var res = await this._magentoSoapService.salesOrderListAsync( sessionId, filters ).ConfigureAwait( false );

			return res;
		}

		public async Task< catalogProductListResponse > GetProductsAsync()
		{
			var sessionId = await this.GetSessionId().ConfigureAwait( false );

			var filters = new filters { filter = new associativeEntity[ 0 ] };

			var store = string.IsNullOrWhiteSpace( this.Store ) ? null : this.Store;

			var res = await this._magentoSoapService.catalogProductListAsync( sessionId, filters, store ).ConfigureAwait( false );

			return res;
		}
	}
}