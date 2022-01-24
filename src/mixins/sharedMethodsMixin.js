import { request } from '../utils/constants'
/* eslint-disable */

//This mixin contains methods and variable names that are being shared across the client, resources and users views
export default {
    data() {
        return {
            allItems: [],
            itemName: null,
            editSelectedItem: false,
            test: [],
            detailsToEdit: {},
            loadSpinner: false,
            isFormContent: false,
            isDetailsContent: false,
            itemId: null,
            selectedItem: {},
            foundItem: [],
            headersOveride: {
                'Authorization': `Bearer ${localStorage.getItem('ACCESS_TOKEN')}`
            }
        }
    },
    created() {
        this.itemName = this.$options.name.toLowerCase()
    },
    methods: {
        turnOnSlideOut() {
            this.$refs.slideout.toggleSlideout();
            this.isFormContent = true;
            this.editSelectedItem = false;
            this.isDetailsContent = false;
        },
        turnOffSlideOut() {
            this.$refs.slideout.closeSlideOut();
            this.isFormContent = false;
            this.isDetailsContent = false;
            this.editSelectedItem = false;
            //Reset selected item
            this.selectedItem = {};
        },
        fetchItems(endpoint) {
            return new Promise((resolve, _) => {
                request.get(`${endpoint}`, {
                    // Overrides the the default bearer. Doesnt reload upon login
                    headers: this.headersOveride
                })
                    .then(res => {
                        resolve(res);
                    }).catch(err => {
                        this.$toaster.error('We\'re having trouble loading. Reload the page and try again')
                    });
            });
        },
        createItem(endpoint, data, uniqueName) {
            this.loadSpinner = true
            return new Promise((resolve, _) => {
                request.post(`${endpoint}`, data, {
                    // Overrides the the default bearer. Doesnt reload upon login
                    headers: this.headersOveride
                })
                    .then(res => {
                        resolve(res);
                        this.loadSpinner = false;
                        this.$toaster.success(`Successfully added ${this.itemName.slice(0, -1)} ${uniqueName}`)
                        //Hide form
                        this.isFormContent = false;
                        // Show details
                        this.isDetailsContent = true;
                    }).catch(err => {
                        this.loadSpinner = false;
                        this.$toaster.success('Sorry ' + err.response.data.Message)
                    });
            });
        },
        displayItemDetails(allItems) {
            this.selectedItem = {};
            let itemId = event.target.dataset.id;
            for (let index = 0; index < allItems.length; index++) {
                if (allItems[index].id == itemId) {
                    this.$refs.slideout.toggleSlideout();
                    this.isFormContent = false;
                    this.isDetailsContent = true;
                    this.editSelectedItem = false;
                    this.selectedItem = allItems[index];
                }
            }
        },
        editItem(itemName) {
            //Get old values
            this.detailsToEdit = itemName
            //Turn off other slideout content
            this.isFormContent = false;
            this.isDetailsContent = false;
            this.editSelectedItem = true;
        },
        setItemToEdit(allItems) {
            this.itemId = event.target.dataset.id;
            //Find matching item in array
            for (let i = 0; i < allItems.length; i++) {
                Object.keys(allItems[i]).forEach((key, index) => {
                    if (allItems[i].id == this.itemId) {
                        //Set client as selected client
                        this.selectedItem = allItems[i];
                    }
                })
            }
            //Call edit client
            this.editItem(this.selectedItem);
            //Turn on slideout
            this.$refs.slideout.toggleSlideout();
        },
        returnDetails(allItems, uniqueName) {
            //Find matching item in given array
            for (let i = 0; i < allItems.length; i++) {
                if (allItems[i].id === parseInt(uniqueName)) {
                    //Set client as selected item
                    this.selectedItem = allItems[i];
                }
            }
            this.isFormContent = false;
            this.isDetailsContent = true;
            this.editSelectedItem = false;
        },
        updateItemDetails(endpoint, details, uniqueName) {
            this.loadSpinner = true;
            return new Promise((resolve, _) => {
                request.put(`${endpoint}`, details, {
                    // Overrides the the default bearer. Doesnt reload upon login
                    headers: this.headersOveride
                })
                    .then(res => {
                        resolve(res);
                        this.loadSpinner = false;
                        this.$toaster.success(`Successfuly saved ${uniqueName}'s details`)
                        //Display added item
                        this.selectedItem = details;
                        // Hide edit template
                        this.editSelectedItem = false,
                            //Hide form
                            this.isFormContent = false;
                        // Show details
                        this.isDetailsContent = true;
                    }).catch(err => {
                        this.loadSpinner = false;
                        this.$toaster.success('Sorry ' + err.response.data.Message)
                    });
            });
        },
        searchInEndpoint(keyword, endpoint) {
            //This caters for searching for a particular id
            return new Promise((resolve, _) => {
                request.get(`${endpoint}/${keyword}`, {
                    // Overrides the the default bearer. Doesnt reload upon login
                    headers: this.headersOveride
                })
                    .then(res => {
                        resolve(res);
                    }).catch(err => {
                        this.foundItem = [];
                    });
            });
        },
        searchInBothArrayAndEndpoint(keyword, allItems, endpoint) {
            this.foundItem = []
            for (let index = 0; index < allItems.length; index++) {
                if (allItems[index].id === parseInt(keyword)) {
                    this.foundItem = [allItems[index]];
                    this.allItems = this.foundItem;
                } else {
                    // search using endpoint
                    this.searchInEndpoint(keyword, endpoint).then(res => {
                        this.foundItem = [res.data];
                    })
                }
            }
            if (this.foundItem.length === 0) {
                this.allItems = [];
            }
            return this.allItems
        },
        filterWithEndpoint(keyword, category, endpoint) {

            return new Promise((resolve, _) => {
                request.get(`${endpoint}?${category}=${keyword}`, {
                    // Overrides the the default bearer. Doesnt reload upon login
                    headers: this.headersOveride
                })
                    .then(res => {
                        resolve(res);
                    }).catch(err => {
                        this.foundItem = [];
                        if (this.itemName === 'users') {
                            this.users = []
                        }
                    });
            });
        },
        deleteItem(endpoint) {
            return new Promise((resolve, _) => {
                request.delete(`${endpoint}`, {
                    // Overrides the the default bearer. Doesnt reload upon login
                    headers: this.headersOveride
                })
                    .then(res => {
                        resolve(res);
                        this.turnOffSlideOut()
                        this.$toaster.success('The user has been deleted')
                    }).catch(err => {
                        this.$toaster.error('We\'re having trouble loading. Reload the page and try again')
                    });
            });
        },
        sendResetPasswordLink(endpoint, data) {
            //this.loadSpinner = true
            return new Promise((resolve, _) => {
                request.post(`${endpoint}`, data, {
                    // Overrides the the default bearer. Doesnt reload upon login
                    headers: this.headersOveride
                })
                    .then(res => {
                        resolve(res);
                        this.loadSpinner = false;
                        this.$toaster.success("New password link has been sent to user via email")

                    }).catch(err => {
                        this.loadSpinner = false;
                        this.$toaster.success('Sorry ' + err.response.data.Message)
                    });
            });
        },
        checkUserExists(endpoint, data) {
            //this.loadSpinner = true
            return new Promise((resolve, _) => {
                request.post(`${endpoint}`, data, {
                    // Overrides the the default bearer. Doesnt reload upon login
                    headers: this.headersOveride
                })
                    .then(res => {
                        resolve(res);
                        //this.loadSpinner = false;
                        //this.$toaster.success("Sorry")

                    }).catch(err => {
                        this.loadSpinner = false;
                        this.$toaster.success('Sorry, something went wrong. Contact systems administrator');
                    });
            });
        },
    }
}
