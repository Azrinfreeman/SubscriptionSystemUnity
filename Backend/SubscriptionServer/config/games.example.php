<?php
declare(strict_types=1);
// This is private server configuration, never a Unity/client price catalog.
return [
    'Your Game' => [
        'name' => 'My Game',
        'portal_configuration' => '', // bpc_... required per game for a multi-game deployment.
        'plans' => [
            'monthly' => ['name'=>'Monthly', 'description'=>'Premium game access, billed monthly.', 'price_id'=>'price_REPLACE_MONTHLY', 'display_price'=>'Set your actual price/currency', 'active'=>true],
            'yearly' => ['name'=>'Yearly', 'description'=>'Premium game access, billed yearly.', 'price_id'=>'price_REPLACE_YEARLY', 'display_price'=>'Set your actual price/currency', 'active'=>true],
        ],
    ],
];
