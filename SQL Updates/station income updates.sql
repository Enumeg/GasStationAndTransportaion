update station_income set sin_price_id = (Select gsp_id from gas_price where gsp_date = (select max(gsp_date) from gas_price where gsp_date <= sin_date) and
  gsp_gas_id = sin_gas_id)