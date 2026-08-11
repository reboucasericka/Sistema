<?php

namespace App\Http\Requests\Api\V1;

use Illuminate\Foundation\Http\FormRequest;

abstract class ProductRequest extends FormRequest
{
    protected function prepareForValidation(): void
    {
        $aliases = [];

        if ($this->has('sale_price')) {
            $aliases['price'] = $this->input('sale_price');
        }

        if ($this->has('purchase_price')) {
            $aliases['cost_price'] = $this->input('purchase_price');
        }

        if ($this->has('current_stock')) {
            $aliases['stock_quantity'] = $this->input('current_stock');
        }

        if ($this->has('minimum_stock')) {
            $aliases['min_stock'] = $this->input('minimum_stock');
        }

        if ($aliases !== []) {
            $this->merge($aliases);
        }
    }
}
