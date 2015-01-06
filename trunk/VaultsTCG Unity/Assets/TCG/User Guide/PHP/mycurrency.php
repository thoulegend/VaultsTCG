<?php

$userid = $_REQUEST['userid']; 
$password = $_REQUEST['password']; 


$host = 'localhost'; 
$user = 'secondhand_tcg';  
$dbpassword = '111111';
$db = 'secondhand_tcg'; 


mysql_connect($host, $user, $dbpassword) or die(mysql_error()); 
mysql_select_db($db); 

$check = mysql_query("SELECT * FROM `silver` WHERE `user_id`='".$userid."'" ) or die (mysql_error());
$numrows = mysql_num_rows($check);
if ($numrows == 0)
{
		die ("User id ".$userid." doesn't have silver \n");
}

else 
{
while ($row = mysql_fetch_assoc($check))  //finds the rows that have our username
{
	  echo $row['silver'] ;
	
}
}


?>
