function dataToJson(data){
    return JSON.stringify(data);
};

function jsonToData(data){ 
    return JSON.parse(data);
};

function getCartData(){
    return localStorage.getItem('cartData');
};

function setCartData(data){
    localStorage.setItem('cartData', data);
};

function getMyCart(){
    let cart = localStorage.getItem('cartData');
    cart = cart ? jsonToData(cart) : [];
    return cart;
}

let allGoods = [];

function getGoods(){
    fetch('db/db.json').then(res => res.json()).then(result => {allGoods = result;})
}

const cart = { 
    cartGoods: getMyCart(),

    addCartId(id){
        const goodItem = this.cartGoods.find(good => good.id === id);
        if (goodItem){
            this.plusGood(id)
        }else{
            const {id: idx,name,price} = allGoods.find(good => good.id === id);
            this.cartGoods.push({id: idx,name,price, count: 1});
            this.cartRender();
        } 
        setCartData(dataToJson(this.cartGoods));

    },
    cartRender(){
        cartTableGoods.textContent = '';
        this.cartGoods.forEach(({name, id, price, count}) => {
            const trGood = document.createElement('tr');
            trGood.className = 'cart-item';
            trGood.dataset.id = id;
            trGood.innerHTML = `
            <td>${name}</td>
            <td>${price}\ ₽</td>
            <td><button class = "cart-btn-minus new-year-btn" data-id = ${id}>-</button></td>
            <td>${count}</td>
            <td><button class = "cart-btn-plus new-year-btn" data-id = ${id}>+</button></td>
            <td>${count * price}</td>
            <td><button class = "cart-btn-delete new-year-btn" data-id = ${id}>x</button></td>
            `
            cartTableGoods.append(trGood);
        })
        const totalPrice = this.cartGoods.reduce((sum, item) => sum + item.price * item.count, 0);
        cartTableTotal.textContent = `${totalPrice}₽`;
        cartCount.textContent = `${totalPrice}₽`;
    },
    plusGood(id){
        const elem = this.cartGoods.find(el => el.id === id);
        if(elem){
            elem.count++;
        }
        setCartData(dataToJson(this.cartGoods));
        this.cartRender(); //перерисовка
    },

    minusGood(id){
        const elem = this.cartGoods.find(el => el.id === id);
        if (elem.count === 1) {
            this.deleteGood(id);
        }else{
            elem.count--;
        }
        setCartData(dataToJson(this.cartGoods));
        this.cartRender();
    },

    deleteGood(id){
        this.cartGoods = this.cartGoods.filter(el => el.id !== id);
        setCartData(dataToJson(this.cartGoods));
        this.cartRender();
    }

}

const buttonCart = document.querySelector('.button-cart'); 
const modalCart = document.querySelector('#modal-cart')
const cartTableGoods = document.querySelector('.cart-table__goods');
const cartTableTotal = document.querySelector('.card-table__total');
const cartCount = document.querySelector('.cart-count');
const navigationLink = document.querySelectorAll('.navigation-link');
const longGoodList = document.querySelector('.long-goods-list');
const specialOffers = document.querySelector('.special-offers'); 
const logoLink = document.querySelector('.logo-link');
const categoryTitle = document.querySelector('.category-title');
const cardMenu = document.querySelector('.card-menu');
const parents = document.querySelectorAll('.navigation-item');
const menu = document.querySelector('.menu');
const menuPresent = document.querySelector('.long-menu-present');

menuPresent.classList.add('blokirovka');

menu.addEventListener('click', ()=>{
    
    renderCards(allGoods);
})

cardMenu.addEventListener('click', () => {
    specialOffers.classList.add('blokirovka');
    document.body.classList.add('v');
    menuPresent.classList.remove('blokirovka');
})


cartTableGoods.addEventListener('click', (e) => {
    const target = e.target;
    if (target.tagName === 'BUTTON'){
        const className = target.className;
        const id = target.dataset.id;
        switch(className) {
            case 'cart-btn-delete new-year-btn':
                cart.deleteGood(id);
                break;
            case 'cart-btn-minus new-year-btn':
                cart.minusGood(id);
                break;
            case 'cart-btn-plus new-year-btn':
                cart.plusGood(id);
                break;
        }
    }
})

function renderCards(data){
    longGoodList.textContent = '';
    const cards = data.map(good => createCard(good));
    longGoodList.append(...cards);
    document.body.classList.add('show-goods');
}

logoLink.addEventListener('click', () => {
    if(document.body.classList.contains('show-goods') || document.body.classList.remove('v')
    ){
        document.body.classList.remove('show-goods');
        document.body.classList.remove('v');
        specialOffers.classList.remove('blokirovka');
    }
    menuPresent.classList.add('blokirovka');
})

function createCard(objCard){
    const card = document.createElement('div');
    card.className = "col-lg-3 col-sm-6";
    card.innerHTML = `
    <div class = "goods-card">
    <img src="db/${objCard.img}" alt="${objCard.name}" class="good-image">
    <h3 class="goods-title">${objCard.name}</h3>
    <button class="button goods-card-btn add-to-card" data-id= "${objCard.id}">
    <span class="button-price">${objCard.price} ₽</span></button>
    </div>`;
    card.addEventListener('click', () =>{
        cart.addCartId(objCard.id);
    })
    return card;
}



function filterCard(field, value){
    renderCards(allGoods.filter(good => good[field] === value))

}

navigationLink.forEach((link) => {
    link.addEventListener('click', (e) => {
        menuPresent.classList.add('blokirovka');
        parents.forEach((p) => p.classList.remove('navigation-item-active'));
        const field = link.dataset.field;
        if (field){ 
            const value = link.textContent;     
            categoryTitle.textContent = `${value}`;         
            filterCard(field, value);
            return;
        }
        else{
            categoryTitle.textContent = 'все меню';
        }
        renderCards(allGoods);
        
    })
})

buttonCart.addEventListener('click', () => {
    modalCart.classList.add('show');
})

document.addEventListener('mouseup', (e) => {
    const target = e.target;
    if (!target.closest('.modal') || target.classList.contains('modal-close')) {
        if(modalCart.classList.contains('show')){
            modalCart.classList.remove('show');
        }
    }
})

document.body.addEventListener('click', (e) => { 
    const target = e.target.closest('.add-to-cart');
    if(target) {
        cart.addCartId(target.dataset.id)
    }
})


getGoods();
cart.cartRender();
