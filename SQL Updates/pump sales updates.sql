update pump_sales ps set pms_price_id = 
 (Select gsp_id from gas_price where gsp_date = (select max(gsp_date) from gas_price where gsp_date <= ps.pms_date) and
  gsp_gas_id = (select pum_gas_id from pumps where pum_id = ps.pms_pum_id)) ;
