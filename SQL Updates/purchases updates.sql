update purchases set pur_price_id = (Select gsp_id from gas_price where gsp_date = (select max(gsp_date) from gas_price 
where gsp_date <= pur_date) and  gsp_gas_id = pur_gas_id)