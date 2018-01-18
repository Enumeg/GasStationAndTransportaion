using Source;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace haies
{
    public class CustomerAccounts
    {
        public static decimal Get_Balance(Customer_type Customer_Type, object Cust_Id, object Per_Id, DateTime DateTime)
        {
            decimal Balance = 0;
            try
            {

                DB db = new DB();
                db.AddCondition("cstl_date", DateTime.Date, false, "<", "Date");
                db.AddCondition("cstl_cust_id", Cust_Id, false, "=", "cst_id");
                db.AddCondition("per_id", Per_Id);
                if (Customer_Type == Customer_type.مصنع)
                {
                    decimal.TryParse(db.Select(@"select COALESCE(sum(cstl_value),0) + (select COALESCE(sum(trc_value),0) from transactions where trc_ref = 0 
                                                and trc_direction = 0 and trc_person=@per_id and trc_date <@Date)
                                                -(select COALESCE(sum((rec_sell_price*rec_amount)+(COALESCE(trs_sell_price,0)*COALESCE(rec_amount,1))
                                                -trs_discount),0)- COALESCE(sum(trs_paid),0) from transportation left join receipt on rec_id= trs_rec_id where trs_date<@Date and trs_cust_id=@cst_id)
                                                 from customer_loans where cstl_date<@Date and cstl_cust_id=@cst_id; ").ToString(), out Balance);
                }
                else
                {
                    decimal.TryParse(db.Select(@"select COALESCE(sum(cstl_value),0)  + (select COALESCE(sum(trc_value),0) from transactions where trc_ref = 0 and 
                                                trc_direction = 0 and trc_person=@per_id and trc_date <@Date)
                                                -(select COALESCE(sum(sin_cost),0) from station_income where sin_date<@Date and sin_cust_id=@cst_id)
                                                 from customer_loans where cstl_date<@Date and cstl_cust_id=@cst_id; ").ToString(), out Balance);

                }
            }
            catch
            {

            }
            return Balance * -1;
        }

        public static decimal[] Get_Customers_Accounts(Customer_type Customer_Type, object Cust_Id, object Per_Id, DateTime From, DateTime To, DataGrid Out_DataGrid, DataGrid In_DataGrid)
        {

            DB db = new DB();
            DataSet ds = new DataSet();
            decimal[] Totals = new decimal[] { 0, 0, 0, 0, 0 };
            try
            {
                db.AddCondition("trs_date", From, false, ">=", "SD");
                db.AddCondition("trs_date", To, false, "<=", "ED");
                db.AddCondition("cust_id", Cust_Id);
                db.AddCondition("per_id", Per_Id);
                if (Customer_Type == Customer_type.مصنع)
                {
                    ds = db.SelectSet(@"select rec_id,rec_number,trs_date,COALESCE(rec_amount,0) rec_amount,trs_paid,trs_discount,c.cem_name,unit_name,pl.pl_name,
                                                p.per_name customer,p2.per_name driver,cs.car_number, trs_payment_method,trs_card_number,
                                                COALESCE(rec_sell_price,0)+COALESCE(trs_sell_price,0)-(trs_discount/COALESCE(rec_amount,1)) unit_price,
                                                Round(COALESCE(rec_sell_price*rec_amount,0)+(COALESCE(trs_sell_price,0)*COALESCE(rec_amount,1))-trs_discount,2) total_price,                                     
                                                Round(COALESCE(rec_sell_price*rec_amount,0)+(COALESCE(trs_sell_price,0)*COALESCE(rec_amount,1))-trs_paid,2) trs_rest 
                                                from transportation t                                                                                            										      
                                                join drivers d on d.dri_id=trs_dri_id
                                                join cars cs on trs_car_id=car_id 
                                                join persons p2 on p2.per_id=d.dri_per_id                                         
                                                left join customer cu on cust_id = trs_cust_id 
                                                left join persons p on p.per_id = cust_per_id                                            
                                                left join places pl on pl.pl_id=t.trs_pl_id              
                                                left join  receipt r on t.trs_rec_id=r.rec_id  
                                                left join cement c on cem_id=rec_cem_id 
                                                left join units on rec_unit_id = unit_id                                
                                                where trs_date>=@SD and trs_date<=@ED and trs_cust_id=@cust_id order by trs_date,rec_number;
                                                select * from customer_loans where cstl_date>=@SD and cstl_date<=@ED and cstl_cust_id=@cust_id union all
                                                select trc_id,trc_person,trc_date,trc_value,trc_description from transactions where trc_ref = 0 and trc_direction = 0 and
                                                trc_person=@per_id and trc_date>=@SD and trc_date<=@ED ");
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        Totals[1] += decimal.Parse(row["total_price"].ToString());
                        if (decimal.Parse(row["trs_paid"].ToString()) > 0)
                        {
                            ds.Tables[1].Rows.Add(null, null, row["trs_date"], row["trs_paid"], row["trs_payment_method"]);
                        }
                    }
                }
                else
                {
                    ds = db.SelectSet(@"select s.*,gas_name from station_income s join gas on gas_id=sin_gas_id where sin_date>=@SD and sin_date<=@ED and sin_cust_id=@cust_id order by sin_date;
                                        select * from customer_loans where cstl_date>=@SD and cstl_date<=@ED and cstl_cust_id=@cust_id  union all
                                        select trc_id,trc_person,trc_date,trc_value,trc_description from transactions where trc_ref = 0 and trc_direction = 0 
                                        and trc_person=@per_id and trc_date>=@SD and trc_date<=@ED order by cstl_date");
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        Totals[1] += decimal.Parse(row["sin_cost"].ToString());
                    }

                }
                foreach (DataRow row in ds.Tables[1].Rows)
                {
                    Totals[2] += decimal.Parse(row["cstl_value"].ToString());
                }
                if (Customer_Type == Customer_type.مصنع)
                {
                    Out_DataGrid.ItemsSource = ds.Tables[0].DefaultView;
                }
                else
                {
                    Out_DataGrid.ItemsSource = ds.Tables[0].DefaultView;
                }                
                In_DataGrid.ItemsSource = ds.Tables[1].DefaultView;
                Totals[0] = Get_Balance(Customer_Type, Cust_Id, Per_Id, From);
                Totals[3] = Totals[1] - Totals[2];
                Totals[4] = Totals[3] + Totals[0];

            }
            catch
            {

            }
            return Totals;
        }

    }
}
