<?php
declare(strict_types=1);
require __DIR__.'/common.php';
runCommand(fn()=>bin2hex(random_bytes(16)));
