<?php
class Config extends Illuminate\Database\Eloquent\Model {
    protected $table = 'config';
    protected $hidden = ['giant_bomb_api_key'];
    protected $appends = ['is_giant_bomb_enabled'];
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

    public function getIsGiantBombEnabledAttribute() {
        return (bool) $this->giant_bomb_api_key;
    }
}
