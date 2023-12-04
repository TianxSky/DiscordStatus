<?php
if (isset($_GET['ip'])) :
    header("Location:" . "steam://connect/". $_GET['ip']);
    exit;
endif;
?>