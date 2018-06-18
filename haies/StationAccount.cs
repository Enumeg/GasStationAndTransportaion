using System;
using System.Data;
using Source;

namespace haies
{
    public class StationAccounts
    {
        public DataView GetGasBalance(DateTime date)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("type");
            dt.Columns.Add("gas1");
            dt.Columns.Add("gas2");
            dt.Columns.Add("gas3");
            dt.Rows.Add("Balance");
            dt.Rows.Add("Purchases");
            dt.Rows.Add("sales");
            dt.Rows.Add("loose");
            dt.Rows.Add("Balance");
            try
            {
                int i = 1;
                DB db = new DB();
                foreach (DataRow row in db.SelectTable("select gas_id,gas_name from gas group by gas_name order by gas_id").Rows)
                {
                    DB sell = new DB("pump_sales");
                    sell.AddCondition("pms_date", date, false, "<", "SD");
                    sell.AddCondition("gas_id", row[0]);
                    decimal.TryParse(sell.Select(@"select COALESCE(sum(pur_amount),0) -
                                      ((select COALESCE(sum(ps.pms_amount),0) from pump_sales ps join pumps p on ps.pms_pum_id=p.pum_id
                                       join gas g on g.gas_id=p.pum_gas_id where pms_date<@SD and pum_gas_id=@gas_id)+
                                      (select COALESCE(sum(gls_amount),0) from gas_lose where gls_date<@SD and gls_gas_id=@gas_id))
                                       from purchases where pur_date<@SD and pur_gas_id=@gas_id;").ToString(), out var currentBalance);

                    DB pur = new DB();
                    pur.AddCondition("pur_date", date, false, "=", "SD");
                    pur.AddCondition("gas_id", row[0]);
                    DataSet ds1 = pur.SelectSet(@"select COALESCE(sum(pur_amount),0) pur_amount from purchases where pur_date=@SD and pur_gas_id=@gas_id;
                                                  select COALESCE(sum(gls_amount),0) gls_amount from gas_lose where gls_date=@SD and gls_gas_id=@gas_id;
                                                  select COALESCE(sum(ps.pms_amount),0) gas_amount from pump_sales ps join pumps p on ps.pms_pum_id=p.pum_id
                                                  join gas g on g.gas_id=p.pum_gas_id where pms_date=@SD and pum_gas_id=@gas_id ;");


                    var purchases = ds1.Tables[0].Rows.Count > 0 ? ds1.Tables[0].Rows[0][0] : 0;
                    var sales = ds1.Tables[1].Rows.Count > 0 ? ds1.Tables[1].Rows[0][0] : 0;



                    DataRow r = ds1.Tables[2].Rows[0];





                    dt.Rows[0][i] = currentBalance;
                    dt.Rows[1][i] = purchases;
                    dt.Rows[2][i] = r["gas_amount"];
                    dt.Rows[3][i] = sales;
                    dt.Rows[4][i] = currentBalance + decimal.Parse(purchases.ToString()) - decimal.Parse(sales.ToString()) - decimal.Parse(r["gas_amount"].ToString());
                    i++;

                }


            }
            catch
            {
                // ignored
            }
            return dt.DefaultView;
        }
        public DataView ListPumps(DateTime date, bool addTotal = true)
        {
            var pumps = new DataTable();
            pumps.Columns.Add("pmr_id"); pumps.Columns.Add("pmr_date"); pumps.Columns.Add("gas_name"); pumps.Columns.Add("pum_number");
            pumps.Columns.Add("pmr_today", typeof(decimal)); pumps.Columns.Add("pmr_yesterday", typeof(decimal));
            pumps.Columns.Add("pmr_amount", typeof(decimal)); pumps.Columns.Add("pmr_value", typeof(decimal));

            decimal tga = 0, tgv = 0;
            try
            {
                var db = new DB();
                db.AddCondition("pmr_date", date, false, "=", "Today");


                var ds = db.SelectSet(@"select pmr_id,pmr_date,gas_name,pum_number,pmr_value from pump_read 
                                    join pumps on pum_id=pmr_pum_id 
                                    join gas on gas_id=pum_gas_id 
                                    where pmr_date=@Today order by gas_id,pmr_pum_id; 
                                    select pms_amount, COALESCE(pms_amount*gsp_sellCost,0) cost, g.gas_id from pump_sales ps 
                                    join pumps p on ps.pms_pum_id=p.pum_id
                                    join gas g on g.gas_id=p.pum_gas_id 
                                    join gas_price on pms_price_id = gsp_id 
                                    where pms_date=@Today
                                    order by g.gas_id , pms_pum_id");
                if (ds.Tables[1].Rows.Count > 0)
                {
                    var gas = ds.Tables[0].Rows[0]["gas_name"].ToString();
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        if (gas != ds.Tables[0].Rows[i]["gas_name"].ToString() && addTotal)
                        {
                            pumps.Rows.Add(null, date.ToString("yyyy/MM/dd"), gas, null, null, null, tga, tgv);
                            tga = 0; tgv = 0;
                        }
                        var todayRead = (decimal)ds.Tables[0].Rows[i]["pmr_value"];
                        var amount = (decimal)ds.Tables[1].Rows[i]["pms_amount"];
                        var read = ds.Tables[1].Rows.Count == i ? 0 : todayRead > amount ? todayRead - amount : Properties.Settings.Default.Pump_Max + todayRead - amount;
                        var value = (decimal)ds.Tables[1].Rows[i]["cost"];
                        pumps.Rows.Add(ds.Tables[0].Rows[i]["pmr_id"], date.ToString("yyyy/MM/dd"), ds.Tables[0].Rows[i]["gas_name"], ds.Tables[0].Rows[i]["pum_number"],
                           todayRead, read, amount, value);
                        tga += amount;
                        tgv += value;
                        gas = ds.Tables[0].Rows[i]["gas_name"].ToString();
                    }
                    if (addTotal)
                        pumps.Rows.Add(null, date.ToString("yyyy/MM/dd"), gas, null, null, null, tga, tgv);

                }

            }
            catch
            {
                // ignored
            }
            return pumps.DefaultView;
        }
        public DataView ListPumps(DateTime from, DateTime to)
        {
            var pumps = new DataTable();
            pumps.Columns.Add("pmr_id");
            pumps.Columns.Add("pmr_date");
            pumps.Columns.Add("gas_name");
            pumps.Columns.Add("pum_number");
            pumps.Columns.Add("pmr_today", typeof(decimal));
            pumps.Columns.Add("pmr_yesterday", typeof(decimal));
            pumps.Columns.Add("pmr_amount", typeof(decimal));
            pumps.Columns.Add("pmr_value", typeof(decimal));
            decimal total = 0;
            var date = from;
            while (date <= to)
            {
                var rows = ListPumps(date, false).Table.Rows;
                foreach (DataRow row in rows)
                {
                    if (rows.IndexOf(row) == rows.Count - 1)
                    {
                        total += decimal.Parse(row["pmr_value"].ToString());
                    }
                    pumps.Rows.Add(row.ItemArray);
                }
                date = date.AddDays(1);
            }
            pumps.Rows.Add(null, null, null, null, null, null, null, total);
            return pumps.DefaultView;
        }
        public DataView ListSales(DateTime from, DateTime? to = null)
        {
            DB db = new DB();
            try
            {
                if (to == null)
                    to = from;
                db.AddCondition("sin_date", from, false, ">=", "SD");
                db.AddCondition("sin_date", to, false, "<=", "ED");
                DataSet ds = db.SelectSet(@"select sin_amount,(sin_amount*gsp_sellCost) as sin_cost,g.gas_name,p.per_name custo , sin_date , sin_id from station_income si
                                            join gas g on gas_id=sin_gas_id                                            
                                            join gas_price on sin_price_id = gsp_id 
                                            join customer cu on cust_id=sin_cust_id join persons p on per_id = cust_per_id where sin_date>=@SD and sin_date <= @ED;
                                            select COALESCE(sum(sin_amount*gsp_sellCost),0) from station_income
                                            join gas_price on sin_price_id = gsp_id where sin_date>=@SD and sin_date <= @ED;");
                ds.Tables[0].Rows.Add(null, ds.Tables[1].Rows[0][0]);
                return ds.Tables[0].DefaultView;
            }
            catch
            {
                return null;
            }

        }
        public DataView ListOutcome(DateTime from, DateTime? to = null)
        {
            DB db = new DB();
            try
            {

                if (to == null)
                    to = from;
                db.AddCondition("sout_date", from, false, ">=", "SD");
                db.AddCondition("sout_date", to, false, "<=", "ED");


                DataSet ds = db.SelectSet(@"select s.* from station_outcome s where sout_date>=@SD and sout_date <=@ED ;
                                            select COALESCE(sum(sout_value),0) from station_outcome where sout_date>=@SD and sout_date <=@ED ;");
                ds.Tables[0].Rows.Add();
                ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["sout_value"] = ds.Tables[1].Rows[0][0];
                return ds.Tables[0].DefaultView;
            }
            catch
            {
                return null;
            }
        }
        public DataView ListPurchases(DateTime from, DateTime? to = null)
        {
            DB db = new DB();
            try
            {
                if (to == null)
                    to = from;
                db.AddCondition("pur_date", from, false, ">=", "SD");
                db.AddCondition("pur_date", to, false, "<=", "ED");

                DataSet ds = db.SelectSet(@"select pur.*,g.gas_name from purchases pur join gas g on pur_gas_id=gas_id                
                                            where pur_date>=@SD and pur_date<=@ED order by pur_date;
                                            select COALESCE(sum(pur_totalCost),0) from purchases where pur_date>=@SD and pur_date<=@ED;");
                ds.Tables[0].Rows.Add();
                ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["pur_totalCost"] = ds.Tables[1].Rows[0][0];
                return ds.Tables[0].DefaultView;
            }
            catch
            {
                return null;
            }
        }
        public DataView ListPayments(DateTime from, DateTime? to = null)
        {
            DB db = new DB();
            try
            {
                if (to == null)
                    to = from;
                db.AddCondition("cstl_date", from, false, ">=", "SD");
                db.AddCondition("cstl_date", to, false, "<=", "ED");

                DataSet ds = db.SelectSet(@"select cl.*,p.per_name name from customer_loans cl join customer c on c.cust_id=cl.cstl_cust_id and cust_type=1                                                   
                                                    join persons p on p.per_id=c.cust_per_id where cstl_date>=@SD and cstl_date<=@ED order by cstl_date; 
                                                    select COALESCE(sum(cstl_value),0) from customer_loans join customer on cust_id=cstl_cust_id and cust_type=1
                                                    where cstl_date>=@SD and cstl_date<=@ED ;");


                ds.Tables[0].Rows.Add();
                ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["cstl_value"] = ds.Tables[1].Rows[0][0];
                return ds.Tables[0].DefaultView;
            }
            catch
            {
                return null;
            }
        }
        public DataView ListBank(DateTime from, DateTime? to = null)
        {
            DB db = new DB();
            try
            {
                if (to == null)
                    to = from;
                db.AddCondition("bnk_date", from, false, ">=", "SD");
                db.AddCondition("bnk_date", to, false, "<=", "ED");

                DataSet ds = db.SelectSet(@"select * from bank where bnk_date>=@SD and bnk_date<=@ED order by bnk_date;
                                            select COALESCE(sum(bnk_value),0) from bank where bnk_date>=@SD and bnk_date<=@ED;");
                ds.Tables[0].Rows.Add();
                ds.Tables[0].Rows[ds.Tables[0].Rows.Count - 1]["bnk_value"] = ds.Tables[1].Rows[0][0];
                return ds.Tables[0].DefaultView;
            }
            catch
            {
                return null;
            }
        }
        public StationAccountModel GetAccounts(DateTime from, DateTime? to = null)
        {
            if (to == null)
                to = from;

            var model = new StationAccountModel();
            try
            {
                DB db = new DB();
                db.AddCondition("sin_date", from, false, ">=", "SD");
                db.AddCondition("sin_date", to, false, "<=", "ED");
                DataSet ds = db.SelectSet(@"select COALESCE(sum(sin_amount*gsp_sellCost),0) from station_income
                                            join gas_price on sin_price_id = gsp_id where  sin_date>=@SD and sin_date<=@ED ;
                                            select COALESCE(sum(sout_value),0) from station_outcome where sout_date>=@SD and sout_date<=@ED;
                                            select COALESCE(sum(pur_amount*gsp_buyCost),0) ,COALESCE(sum(pur_amount*(gsp_buyCost*gsp_buy_tax/(gsp_buy_tax+100))),0)
                                            from purchases 
                                            join gas_price on pur_price_id = gsp_id where pur_date>=@SD and pur_date<=@ED;
                                            select COALESCE(sum(bnk_value),0) from bank where bnk_date>=@SD and bnk_date<=@ED;
                                            select COALESCE(sum(cstl_value),0) from customer_loans join customer on cust_id=cstl_cust_id and cust_type=1 where 
                                            cstl_date>=@SD and cstl_date<=@ED;
                                            select COALESCE(sum(pms_amount*gsp_sellCost),0),COALESCE(sum(pms_amount*(gsp_sellCost*gsp_sell_tax/(gsp_sell_tax+100))),0) 
                                            from pump_sales ps                                             
                                            join gas_price on pms_price_id = gsp_id 
                                            where pms_date>=@SD and pms_date<=@ED;");
                model.LastBalance = GetLastBalance(from);
                model.Sales = decimal.Parse(ds.Tables[5].Rows[0][0].ToString());
                model.SalesTax = decimal.Parse(ds.Tables[5].Rows[0][1].ToString());
                model.NetSales = model.Sales - model.SalesTax;
                model.FutureSales = decimal.Parse(ds.Tables[0].Rows[0][0].ToString());
                model.CashSales = model.Sales - model.FutureSales;
                model.Outcome = decimal.Parse(ds.Tables[1].Rows[0][0].ToString());
                model.Purchases = decimal.Parse(ds.Tables[2].Rows[0][0].ToString());
                model.PurchasesTax = decimal.Parse(ds.Tables[2].Rows[0][1].ToString());
                model.NetPurchases = model.Purchases - model.PurchasesTax;
                model.Bank = decimal.Parse(ds.Tables[3].Rows[0][0].ToString());
                model.Payment = decimal.Parse(ds.Tables[4].Rows[0][0].ToString());
                model.Income = model.CashSales + model.Payment - model.Outcome;
                model.Balance = model.CashSales + model.LastBalance + model.Payment - model.Outcome - model.Bank;
            }
            catch
            {
                // ignored
            }
            return model;
        }
        decimal GetLastBalance(DateTime date)
        {
            decimal balance = 0;
            try
            {
                DB db = new DB();
                db.AddCondition("pmr_date", date, false, "<", "Today");
                decimal.TryParse(db.Select(@"select COALESCE(sum(pms_amount*gsp_sellCost),0) 
                                         + (select COALESCE(sum(cstl_value),0) from customer_loans join customer on cust_id=cstl_cust_id and cust_type=1 where cstl_date<@Today)
                                         -((select COALESCE(sum(sin_amount*gsp_sellCost),0) from station_income
                                            join gas_price on sin_price_id = gsp_id where sin_date<@Today)                                            
                                         + (select COALESCE(sum(sout_value),0) from station_outcome where sout_date<@Today)                                            
                                         + (select COALESCE(sum(bnk_value),0) from bank where bnk_date<@Today))                                           
                                         from pump_sales ps join pumps p on ps.pms_pum_id=p.pum_id
                                         join gas_price on pms_price_id = gsp_id where pms_date<@Today;").ToString(), out balance);
            }
            catch
            {
                // ignored
            }
            return balance;
        }

    }

    public class StationAccountModel
    {
        public decimal LastBalance { get; set; }
        public decimal FutureSales { get; set; }
        public decimal CashSales { get; set; }
        public decimal Sales { get; set; }
        public decimal NetSales { get; set; }
        public decimal SalesTax { get; set; }
        public decimal Outcome { get; set; }
        public decimal Purchases { get; set; }
        public decimal NetPurchases { get; set; }
        public decimal PurchasesTax { get; set; }
        public decimal Bank { get; set; }
        public decimal Payment { get; set; }
        public decimal Income { get; set; }
        public decimal Balance { get; set; }

    }
}
