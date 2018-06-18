update station_income set sin_amount = truncate(sin_cost / (select gsp_sellCost from gas_price where gsp_id = sin_price_id ) , 3)  
