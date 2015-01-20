<?php
class Config extends Illuminate\Database\Eloquent\Model {
    protected $table = 'config';
    public $timestamps = false;

    public function defaultUser() {
        return $this->hasOne('User', 'id', 'default_user');
    }
}
