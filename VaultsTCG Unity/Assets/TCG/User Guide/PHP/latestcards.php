<?php



$host = 'localhost'; 
$user = 'secondhand_tcg';  
$dbpassword = '111111';
$db = 'secondhand_tcg'; 
 

mysql_connect($host, $user, $dbpassword) or die(mysql_error());
mysql_select_db($db); 

$check = mysql_query("SELECT * FROM `promo_cards` " ) or die (mysql_error());
$numrows = mysql_num_rows($check);
if ($numrows == 0)
{
		die ("no promo cards found \n");
}

else 
{
$msg="";
while ($row = mysql_fetch_assoc($check))  //finds the rows that have our username
{
	  $msg .= $row['card_id'].",".$row['cost']."\n";
	
}
echo $msg;
}


?>
