<?php
class Config extends Illuminate\Database\Eloquent\Model {
    protected $table = 'config';
    protected $hidden = ['giant_bomb_api_key'];
    protected $appends = ['is_assisted_creation_enabled'];
    public $timestamps = false;

    public function defaultUser() {
        return $this->hasOne('User', 'id', 'default_user');
    }

    public function getIdAttribute($value) {
        return (int) $value;
    }

    public function getDefaultUserAttribute($value) {
        return (int) $value;
    }

    public function getIsAssistedCreationEnabledAttribute() {
        return (bool) $this->giant_bomb_api_key;
    }
}
