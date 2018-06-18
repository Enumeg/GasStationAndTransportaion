ALTER TABLE `purchases`
	ADD COLUMN `pur_price_id` INT(11) NULL DEFAULT NULL AFTER `pur_amount`;
