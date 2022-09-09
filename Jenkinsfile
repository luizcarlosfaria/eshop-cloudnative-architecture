pipeline {
    
    agent none

    environment {
        COMPOSE_PROJECT_NAME = "${env.JOB_NAME}-${env.BUILD_ID}"
    }
    stages {
      
        stage('Build') {

            agent {
                dockerfile {
                    // alwaysPull false
                    // image 'microsoft/dotnet:2.2-sdk'
                    // reuseNode false
                    args '-u root:root'
                }
            }

            steps {
                
                echo sh(script: 'env|sort', returnStdout: true)

                sh 'dotnet build ./eShopCloudNative.Architecture.sln'

            }

        }
        
        stage('Setup databases') {
            
            agent any
            
            steps {

                    sh  '''

                        #docker-compose up -d

                        #sleep 60

                        '''
            }

        }


        stage('Test') {

            agent {
                dockerfile {
                    // alwaysPull false
                    // image 'microsoft/dotnet:2.2-sdk'
                    // reuseNode false
                    args '-u root:root -v /var/run/docker.sock:/var/run/docker.sock'
                }
            }

            steps {

                 withCredentials([usernamePassword(credentialsId: 'SonarQube', passwordVariable: 'SONARQUBE_KEY', usernameVariable: 'SONARQUBE_USERNAME' )]) {

                    sh  '''
                       
                        dotnet test ./eShopCloudNative.Architecture.Tests/eShopCloudNative.Architecture.Tests.csproj \
                            --configuration Debug \
                            --output ./output-tests  \
                            /p:CollectCoverage=true \
                            /p:CoverletOutputFormat=opencover \
                            /p:CoverletOutput='./output-coverage/coverage.xml' \
                            /p:Exclude="[Oragon.*.Tests]*"

                        dotnet sonarscanner begin \
                            /o:luizcarlosfaria \
                            /k:"luizcarlosfaria_eshop-cloudnative-architecture" \
                            /d:sonar.login="$SONARQUBE_KEY" \
                            /d:sonar.host.url="https://sonarcloud.io" \
                            /d:sonar.cs.opencover.reportsPaths="./output-coverage/coverage.xml" \
                            /d:sonar.coverage.exclusions="tests/**/*,Examples/**/*,**/*.CodeGen.cs" \
                                /d:sonar.test.exclusions="tests/**/*,Examples/**/*,**/*.CodeGen.cs" \
                                     /d:sonar.exclusions="tests/**/*,Examples/**/*,**/*.CodeGen.cs"
                        
                        dotnet build ./eShopCloudNative.Architecture.sln
                        dotnet sonarscanner end /d:sonar.login="$SONARQUBE_KEY"
                        '''

                }
                
            }

        }

        stage('Pack') {

            agent {
                dockerfile {
                    // alwaysPull false
                    // image 'microsoft/dotnet:2.2-sdk'
                    // reuseNode false
                    args '-u root:root'
                }
            }

            when { buildingTag() }

            steps {

                script{

                    def projetcs = [
                        './OreShopCloudNative.Architecture.Bootstrap/eShopCloudNative.Architecture.Bootstrap.csproj',
						'./eShopCloudNative.Architecture.Data/eShopCloudNative.Architecture.Data.csproj'
                    ]

                    if (env.BRANCH_NAME.endsWith("-alpha")) {

                        for (int i = 0; i < projetcs.size(); ++i) {
                            sh "dotnet pack ${projetcs[i]} --configuration Debug /p:PackageVersion=${BRANCH_NAME} --include-source --include-symbols --output ../output-packages"
                        }

                    } else if (env.BRANCH_NAME.endsWith("-beta")) {

                        for (int i = 0; i < projetcs.size(); ++i) {
                            sh "dotnet pack ${projetcs[i]} --configuration Release /p:PackageVersion=${BRANCH_NAME} --output ../output-packages"                        
                        }

                    } else {

                        for (int i = 0; i < projetcs.size(); ++i) {
                            sh "dotnet pack ${projetcs[i]} --configuration Release /p:PackageVersion=${BRANCH_NAME} --output ../output-packages"                        
                        }

                    }

                }

            }

        }

        stage('Publish') {

            agent {
                dockerfile {
                    // alwaysPull false
                    // image 'microsoft/dotnet:2.2-sdk'
                    // reuseNode false
                    args '-u root:root'
                }
            }

            when { buildingTag() }

            steps {
                
                script {
                    
                    def publishOnNuGet = ( env.BRANCH_NAME.endsWith("-alpha") == false );
                        
                        withCredentials([usernamePassword(credentialsId: 'myget-oragon', passwordVariable: 'MYGET_KEY', usernameVariable: 'DUMMY' )]) {

                        sh 'for pkg in ./output-packages/*.nupkg ; do dotnet nuget push "$pkg" -k "$MYGET_KEY" -s https://www.myget.org/F/oragon/api/v3/index.json -ss https://www.myget.org/F/oragon/symbols/api/v2/package ; done'
						
                        }

                    if (publishOnNuGet) {

                        withCredentials([usernamePassword(credentialsId: 'nuget-luizcarlosfaria', passwordVariable: 'NUGET_KEY', usernameVariable: 'DUMMY')]) {

                            sh 'for pkg in ./output-packages/*.nupkg ; do dotnet nuget push "$pkg" -k "$NUGET_KEY" -s https://api.nuget.org/v3/index.json ; done'

                        }

                    }                    
				}
            }
        }
    }
    post {

        always {
            node('master'){
                
                sh  '''
                
                #docker-compose down -v

                '''


            }
        }
    }
}